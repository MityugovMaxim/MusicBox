using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UILevelMenu : UIMenu, IInitializable, IDisposable, IPointerDownHandler, IDragHandler, IDropHandler
{
	[SerializeField] AnimationCurve           m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] UILevelPreviewBackground m_Background;
	[SerializeField] UILevelPreviewThumbnail  m_Thumbnail;
	[SerializeField] UIScoreRank              m_ScoreRank;
	[SerializeField] UILevelPreviewLabel      m_Label;

	SignalBus      m_SignalBus;
	LevelProcessor m_LevelProcessor;
	UILoadingMenu  m_LoadingMenu;
	string         m_LevelID;
	IEnumerator    m_RepositionRoutine;

	[Inject]
	public void Construct(
		SignalBus      _SignalBus,
		LevelProcessor _LevelProcessor,
		UILoadingMenu  _LoadingMenu
	)
	{
		m_SignalBus      = _SignalBus;
		m_LevelProcessor = _LevelProcessor;
		m_LoadingMenu    = _LoadingMenu;
	}

	void IInitializable.Initialize()
	{
		m_SignalBus.Subscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void IDisposable.Dispose()
	{
		m_SignalBus.Unsubscribe<LevelStartSignal>(RegisterLevelStart);
	}

	void RegisterLevelStart(LevelStartSignal _Signal)
	{
		Hide(true);
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Background.Setup(m_LevelID, true);
		m_Thumbnail.Setup(m_LevelID);
		m_ScoreRank.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
	}

	public void Next()
	{
		m_LevelID = m_LevelProcessor.GetNextLevelID(m_LevelID);
		
		m_Background.Setup(m_LevelID);
		m_Thumbnail.Setup(m_LevelID);
		m_ScoreRank.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
	}

	public void Previous()
	{
		m_LevelID = m_LevelProcessor.GetPreviousLevelID(m_LevelID);
		
		m_Background.Setup(m_LevelID);
		m_Thumbnail.Setup(m_LevelID);
		m_ScoreRank.Setup(m_LevelID);
		m_Label.Setup(m_LevelID);
	}

	public void Play()
	{
		m_LoadingMenu.Setup(m_LevelID);
		m_LoadingMenu.Show();
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

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
	}

	void IDropHandler.OnDrop(PointerEventData _EventData)
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