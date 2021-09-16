using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Menu(MenuType.ShopMenu)]
public class UIShopMenu : UIMenu, IInitializable, IDisposable
{
	[SerializeField] UIShopMenuItem m_Item;
	[SerializeField] UILoader       m_Loader;
	[SerializeField] RectTransform  m_Container;
	[SerializeField] CanvasGroup    m_ItemsGroup;
	[SerializeField] CanvasGroup    m_LoaderGroup;
	[SerializeField] CanvasGroup    m_ErrorGroup;

	SignalBus              m_SignalBus;
	PurchaseProcessor      m_PurchaseProcessor;
	MenuProcessor          m_MenuProcessor;
	UIShopMenuItem.Factory m_ItemFactory;

	IEnumerator m_ItemsRoutine;
	IEnumerator m_LoaderRoutine;
	IEnumerator m_ErrorRoutine;
	string[]    m_ProductIDs;

	readonly List<UIShopMenuItem> m_Items = new List<UIShopMenuItem>();

	[Inject]
	public void Construct(
		SignalBus              _SignalBus,
		PurchaseProcessor      _PurchaseProcessor,
		MenuProcessor          _MenuProcessor,
		UIShopMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_MenuProcessor     = _MenuProcessor;
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

	public async void Back()
	{
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.ShopMenu);
	}

	public void Restore()
	{
		m_PurchaseProcessor.RestorePurchases();
	}

	public void Reload()
	{
		DisableItems(false);
		DisableError(false);
		EnableLoader(false);
		m_Loader.Restore();
		m_Loader.Play();
		
		void OnInitialize(bool _Success)
		{
			if (_Success)
			{
				Refresh();
				EnableItems(false);
			}
			else
			{
				EnableError(false);
			}
			
			DisableLoader(false);
		}
		
		void ReloadInternal()
		{
			m_PurchaseProcessor.LoadStore();
			
			m_PurchaseProcessor.OnInitialize += OnInitialize;
		}
		
		StartCoroutine(DelayRoutine(2.5f, ReloadInternal));
	}

	protected override void OnShowStarted()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		if (m_PurchaseProcessor.Initialized)
		{
			Refresh();
			DisableLoader(true);
			DisableError(true);
			EnableItems(true);
			return;
		}
		
		DisableItems(true);
		DisableError(true);
		EnableLoader(true);
		m_Loader.Restore();
		m_Loader.Play();
		
		void OnInitialize(bool _Success)
		{
			if (_Success)
			{
				Refresh();
				EnableItems(false);
			}
			else
			{
				EnableError(false);
			}
			
			DisableLoader(false);
		}
		
		void OnShowStartedInternal()
		{
			m_PurchaseProcessor.OnInitialize += OnInitialize;
		}
		
		StartCoroutine(DelayRoutine(0.75f, OnShowStartedInternal));
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

	void DisableError(bool _Instant)
	{
		if (m_ErrorRoutine != null)
			StopCoroutine(m_ErrorRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ErrorRoutine = DisableGroupRoutine(m_ErrorGroup, 0.3f);
			
			StartCoroutine(m_ErrorRoutine);
		}
		else
		{
			m_ErrorGroup.alpha          = 0;
			m_ErrorGroup.interactable   = false;
			m_ErrorGroup.blocksRaycasts = false;
		}
	}

	void EnableError(bool _Instant)
	{
		if (m_ErrorRoutine != null)
			StopCoroutine(m_ErrorRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_ErrorRoutine = EnableGroupRoutine(m_ErrorGroup, 0.3f);
			
			StartCoroutine(m_ErrorRoutine);
		}
		else
		{
			m_ErrorGroup.alpha          = 1;
			m_ErrorGroup.interactable   = true;
			m_ErrorGroup.blocksRaycasts = true;
		}
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

	static IEnumerator DelayRoutine(float _Delay, Action _Callback)
	{
		if (_Delay > float.Epsilon)
			yield return new WaitForSeconds(_Delay);
		
		_Callback?.Invoke();
	}
}