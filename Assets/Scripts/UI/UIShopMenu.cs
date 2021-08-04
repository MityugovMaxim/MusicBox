using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIShopMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UILoader       m_Loader;
	[SerializeField] CanvasGroup    m_PurchasesGroup;
	[SerializeField] CanvasGroup    m_LoaderGroup;
	[SerializeField] UIShopMenuItem m_Purchase;
	[SerializeField] RectTransform  m_Container;

	SignalBus                  m_SignalBus;
	PurchaseProcessor          m_PurchaseProcessor;
	UIShopMenuItem.Factory m_PurchaseFactory;

	IEnumerator m_WaitRoutine;
	IEnumerator m_PurchasesRoutine;
	IEnumerator m_LoaderRoutine;
	string[]    m_PurchaseIDs;

	readonly List<UIShopMenuItem> m_Purchases = new List<UIShopMenuItem>();

	[Inject]
	public void Construct(
		SignalBus                  _SignalBus,
		PurchaseProcessor          _PurchaseProcessor,
		UIShopMenuItem.Factory _PurchaseFactory
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_PurchaseFactory   = _PurchaseFactory;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<PurchaseSignal>(RegisterPurchase);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<PurchaseSignal>(RegisterPurchase);
	}

	void RegisterPurchase(PurchaseSignal _Signal)
	{
		Refresh();
	}

	protected override void OnShowStarted()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		if (m_PurchaseProcessor.Initialized)
		{
			Refresh();
			DisableLoader(true);
			EnablePurchases(true);
			return;
		}
		
		DisablePurchases(true);
		EnableLoader(true);
		m_Loader.Restore();
		m_Loader.Play();
		
		if (m_WaitRoutine != null)
			StopCoroutine(m_WaitRoutine);
		
		m_WaitRoutine = WaitRoutine(
			() => m_PurchaseProcessor.Initialized,
			() =>
			{
				Refresh();
				DisableLoader(false);
				EnablePurchases(false);
			}
		);
	}

	void Refresh()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		m_PurchaseIDs = m_PurchaseProcessor.GetProductIDs();
		
		int delta = m_PurchaseIDs.Length - m_Purchases.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIShopMenuItem purchase = m_PurchaseFactory.Create(m_Purchase);
				purchase.RectTransform.SetParent(m_Container, false);
				m_Purchases.Add(purchase);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int                index = m_Purchases.Count - 1;
				UIShopMenuItem track = m_Purchases[index];
				Destroy(track.gameObject);
				m_Purchases.RemoveAt(index);
			}
		}
		
		foreach (UIShopMenuItem purchase in m_Purchases)
			purchase.gameObject.SetActive(false);
		
		for (var i = 0; i < m_PurchaseIDs.Length; i++)
		{
			UIShopMenuItem purchase   = m_Purchases[i];
			string         purchaseID = m_PurchaseIDs[i];
			
			purchase.Setup(purchaseID);
			
			purchase.gameObject.SetActive(true);
		}
	}

	void DisableLoader(bool _Instant)
	{
		if (m_LoaderRoutine != null)
			StopCoroutine(m_LoaderRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_LoaderRoutine = DisableGroupRoutine(m_LoaderGroup, 0.3f);
			
			StartCoroutine(m_LoaderRoutine);
		}
		else
		{
			m_LoaderGroup.alpha          = 0;
			m_LoaderGroup.interactable   = false;
			m_LoaderGroup.blocksRaycasts = false;
		}
	}

	void EnableLoader(bool _Instant)
	{
		if (m_LoaderRoutine != null)
			StopCoroutine(m_LoaderRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_LoaderRoutine = EnableGroupRoutine(m_LoaderGroup, 0.3f);
			
			StartCoroutine(m_LoaderRoutine);
		}
		else
		{
			m_LoaderGroup.alpha          = 1;
			m_LoaderGroup.interactable   = true;
			m_LoaderGroup.blocksRaycasts = true;
		}
	}

	void DisablePurchases(bool _Instant)
	{
		if (m_PurchasesRoutine != null)
			StopCoroutine(m_PurchasesRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_PurchasesRoutine = DisableGroupRoutine(m_PurchasesGroup, 0.3f);
			
			StartCoroutine(m_PurchasesRoutine);
		}
		else
		{
			m_PurchasesGroup.alpha          = 0;
			m_PurchasesGroup.interactable   = false;
			m_PurchasesGroup.blocksRaycasts = false;
		}
	}

	void EnablePurchases(bool _Instant)
	{
		if (m_PurchasesRoutine != null)
			StopCoroutine(m_PurchasesRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_PurchasesRoutine = EnableGroupRoutine(m_PurchasesGroup, 0.3f);
			
			StartCoroutine(m_PurchasesRoutine);
		}
		else
		{
			m_PurchasesGroup.alpha          = 1;
			m_PurchasesGroup.interactable   = true;
			m_PurchasesGroup.blocksRaycasts = true;
		}
	}

	static IEnumerator WaitRoutine(Func<bool> _Predicate, Action _Callback)
	{
		yield return new WaitUntil(_Predicate);
		
		_Callback?.Invoke();
	}

	static IEnumerator DisableGroupRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = 0;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
		
		_CanvasGroup.interactable   = false;
		_CanvasGroup.blocksRaycasts = false;
	}

	static IEnumerator EnableGroupRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.interactable   = true;
		_CanvasGroup.blocksRaycasts = true;
		
		float source = _CanvasGroup.alpha;
		float target = 1;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}
}