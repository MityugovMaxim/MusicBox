using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIProductMenu : UIMenu, IInitializable, IDisposable, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[SerializeField] AnimationCurve             m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] UIProductMenuItem          m_Item;
	[SerializeField] GameObject                 m_ItemsGroup;
	[SerializeField] RectTransform              m_Container;
	[SerializeField] UIProductPreviewBackground m_Background;
	[SerializeField] UIProductPreviewThumbnail  m_Thumbnail;
	[SerializeField] UIProductPreviewLabel      m_Label;
	[SerializeField] UIProductPreviewPrice      m_Price;
	[SerializeField] LevelPreviewAudioSource    m_PreviewSource;

	SignalBus                 m_SignalBus;
	PurchaseProcessor         m_PurchaseProcessor;
	LevelProcessor            m_LevelProcessor;
	MenuProcessor             m_MenuProcessor;
	HapticProcessor           m_HapticProcessor;
	UIProductMenuItem.Factory m_ItemFactory;

	string      m_ProductID;
	IEnumerator m_RepositionRoutine;

	readonly List<UIProductMenuItem> m_Items = new List<UIProductMenuItem>();

	[Inject]
	public void Construct(
		SignalBus                 _SignalBus,
		PurchaseProcessor         _PurchaseProcessor,
		LevelProcessor            _LevelProcessor,
		MenuProcessor             _MenuProcessor,
		HapticProcessor           _HapticProcessor,
		UIProductMenuItem.Factory _ItemFactory
	)
	{
		m_SignalBus         = _SignalBus;
		m_PurchaseProcessor = _PurchaseProcessor;
		m_LevelProcessor    = _LevelProcessor;
		m_MenuProcessor     = _MenuProcessor;
		m_HapticProcessor   = _HapticProcessor;
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

	void RegisterPurchase()
	{
		Hide();
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Background.Setup(m_ProductID, true);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	public void Next()
	{
		m_ProductID = m_PurchaseProcessor.GetNextProductID(m_ProductID);
		
		m_Background.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	public void Previous()
	{
		m_ProductID = m_PurchaseProcessor.GetPreviousProductID(m_ProductID);
		
		m_Background.Setup(m_ProductID);
		m_Thumbnail.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
		
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	public void Purchase()
	{
		m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		m_PurchaseProcessor.Purchase(
			m_ProductID,
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu),
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu),
			_ProductID => m_MenuProcessor.Hide(MenuType.ProcessingMenu)
		);
		
		m_PreviewSource.Stop();
		
		m_HapticProcessor.Process(Haptic.Type.ImpactLight);
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	protected override void OnShowStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		Refresh();
	}

	protected override void OnHideStarted()
	{
		m_PreviewSource.Stop();
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
	}

	void Refresh()
	{
		if (m_PurchaseProcessor == null)
			return;
		
		string[] levelIDs = m_PurchaseProcessor.GetLevelIDs(m_ProductID)
			.Where(m_LevelProcessor.Contains)
			.ToArray();
		
		if (levelIDs.Length == 0)
		{
			m_ItemsGroup.SetActive(false);
			return;
		}
		
		m_ItemsGroup.SetActive(true);
		
		foreach (UIProductMenuItem item in m_Items)
			item.Stop();
		
		int delta = levelIDs.Length - m_Items.Count;
		int count = Mathf.Abs(delta);
		
		if (delta > 0)
		{
			for (int i = 0; i < count; i++)
			{
				UIProductMenuItem item = m_ItemFactory.Create(m_Item);
				item.RectTransform.SetParent(m_Container, false);
				m_Items.Add(item);
			}
		}
		else if (delta < 0)
		{
			for (int i = 0; i < count; i++)
			{
				int               index = m_Items.Count - 1;
				UIProductMenuItem item  = m_Items[index];
				Destroy(item.gameObject);
				m_Items.RemoveAt(index);
			}
		}
		
		foreach (UIProductMenuItem item in m_Items)
			item.gameObject.SetActive(false);
		
		for (var i = 0; i < levelIDs.Length; i++)
		{
			UIProductMenuItem item    = m_Items[i];
			string            levelID = levelIDs[i];
			
			item.Setup(levelID, PlayPreview, StopPreview);
			
			item.gameObject.SetActive(true);
		}
	}

	void PlayPreview(string _LevelID)
	{
		foreach (UIProductMenuItem item in m_Items)
		{
			if (item.LevelID != _LevelID)
				item.Stop();
		}
		
		m_PreviewSource.Play(_LevelID);
	}

	void StopPreview(string _LevelID)
	{
		m_PreviewSource.Stop();
	}

	void Expand()
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
		
		m_RepositionRoutine = ExpandRoutine(RectTransform, ShowDuration);
		
		StartCoroutine(m_RepositionRoutine);
	}

	void Shrink()
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
		
		m_RepositionRoutine = ShrinkRoutine(RectTransform, HideDuration);
		
		StartCoroutine(m_RepositionRoutine);
	}

	protected override IEnumerator ShowAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.alpha = 1;
		
		yield return ExpandRoutine(RectTransform, _Duration);
	}

	protected override IEnumerator HideAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.alpha = 1;
		
		yield return ShrinkRoutine(RectTransform, _Duration);
	}

	protected override void InstantShow(CanvasGroup _CanvasGroup)
	{
		base.InstantShow(_CanvasGroup);
		
		RectTransform.anchorMin = Vector2.zero;
		RectTransform.anchorMax = Vector2.one;
	}

	protected override void InstantHide(CanvasGroup _CanvasGroup)
	{
		base.InstantHide(_CanvasGroup);
		
		RectTransform.anchorMin = new Vector2(0, -1);
		RectTransform.anchorMax = new Vector2(1, 0);
	}

	IEnumerator ExpandRoutine(RectTransform _RectTransform, float _Duration)
	{
		if (_RectTransform == null)
			yield break;
		
		Vector2 sourceMin = _RectTransform.anchorMin;
		Vector2 sourceMax = _RectTransform.anchorMax;
		Vector2 targetMin = Vector2.zero;
		Vector2 targetMax = Vector2.one;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / _Duration);
			
			_RectTransform.anchorMin = Vector2.Lerp(sourceMin, targetMin, phase);
			_RectTransform.anchorMax = Vector2.Lerp(sourceMax, targetMax, phase);
		}
		
		_RectTransform.anchorMin = targetMin;
		_RectTransform.anchorMax = targetMax;
		
		Show(true);
	}

	IEnumerator ShrinkRoutine(RectTransform _RectTransform, float _Duration)
	{
		if (_RectTransform == null)
			yield break;
		
		Vector2 sourceMin = _RectTransform.anchorMin;
		Vector2 sourceMax = _RectTransform.anchorMax;
		Vector2 targetMin = new Vector2(0, -1);
		Vector2 targetMax = new Vector2(1, 0);
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / _Duration);
			
			_RectTransform.anchorMin = Vector2.Lerp(sourceMin, targetMin, phase);
			_RectTransform.anchorMax = Vector2.Lerp(sourceMax, targetMax, phase);
		}
		
		_RectTransform.anchorMin = targetMin;
		_RectTransform.anchorMax = targetMax;
		
		Hide(true);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		float delta = _EventData.delta.y / Screen.height;
		
		Vector2 min = RectTransform.anchorMin;
		Vector2 max = RectTransform.anchorMax;
		
		min.y = Mathf.Clamp(min.y + delta, -1, 0);
		max.y = Mathf.Clamp(max.y + delta, 0, 1);
		
		RectTransform.anchorMin = min;
		RectTransform.anchorMax = max;
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		const float anchorThreshold = 0.7f;
		const float speedThreshold  = 0.7f;
		
		float speed = _EventData.delta.y / Screen.height / Time.deltaTime;
		
		Vector2 anchor = RectTransform.anchorMax;
		
		if (speed > speedThreshold)
			Expand();
		else if (speed < -speedThreshold)
			Shrink();
		else if (anchor.y > anchorThreshold)
			Expand();
		else
			Shrink();
	}
}