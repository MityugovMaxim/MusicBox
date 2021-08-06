using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIShopMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIShopMenuItem m_Item;
	[SerializeField] UILoader       m_Loader;
	[SerializeField] RectTransform  m_Container;
	[SerializeField] CanvasGroup    m_ItemsGroup;
	[SerializeField] CanvasGroup    m_LoaderGroup;

	SignalBus              m_SignalBus;
	PurchaseProcessor      m_PurchaseProcessor;
	UIShopMenuItem.Factory m_ItemFactory;

	IEnumerator m_WaitRoutine;
	IEnumerator m_ItemsRoutine;
	IEnumerator m_LoaderRoutine;
	string[]    m_ProductIDs;

	readonly List<UIShopMenuItem> m_Items = new List<UIShopMenuItem>();

	[Inject]
	public void Construct(
		SignalBus              _SignalBus,
		PurchaseProcessor      _PurchaseProcessor,
		UIShopMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_ItemFactory       = _ItemFactory;
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

	public void Back()
	{
		Hide();
	}

	public void Restore()
	{
		m_PurchaseProcessor.RestorePurchases();
	}

	protected override void OnShowStarted()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		if (m_PurchaseProcessor.Initialized)
		{
			Refresh();
			DisableLoader(true);
			EnableItems(true);
			return;
		}
		
		DisableItems(true);
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
				EnableItems(false);
				DisableLoader(false);
			}
		);
	}

	void Refresh()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		m_ProductIDs = m_PurchaseProcessor.GetProductIDs();
		
		int delta = m_ProductIDs.Length - m_Items.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIShopMenuItem item = m_ItemFactory.Create(m_Item);
				item.RectTransform.SetParent(m_Container, false);
				m_Items.Add(item);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int            index = m_Items.Count - 1;
				UIShopMenuItem item  = m_Items[index];
				Destroy(item.gameObject);
				m_Items.RemoveAt(index);
			}
		}
		
		foreach (UIShopMenuItem item in m_Items)
			item.gameObject.SetActive(false);
		
		for (var i = 0; i < m_ProductIDs.Length; i++)
		{
			string productID = m_ProductIDs[i];
			
			UIShopMenuItem item = m_Items[i];
			
			item.Setup(productID);
			
			item.gameObject.SetActive(true);
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

	void DisableItems(bool _Instant)
	{
		if (m_ItemsRoutine != null)
			StopCoroutine(m_ItemsRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ItemsRoutine = DisableGroupRoutine(m_ItemsGroup, 0.3f);
			
			StartCoroutine(m_ItemsRoutine);
		}
		else
		{
			m_ItemsGroup.alpha          = 0;
			m_ItemsGroup.interactable   = false;
			m_ItemsGroup.blocksRaycasts = false;
		}
	}

	void EnableItems(bool _Instant)
	{
		if (m_ItemsRoutine != null)
			StopCoroutine(m_ItemsRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ItemsRoutine = EnableGroupRoutine(m_ItemsGroup, 0.3f);
			
			StartCoroutine(m_ItemsRoutine);
		}
		else
		{
			m_ItemsGroup.alpha          = 1;
			m_ItemsGroup.interactable   = true;
			m_ItemsGroup.blocksRaycasts = true;
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