using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISlideMenu : UIMenu, IPointerDownHandler, IDragHandler, IDropHandler
{
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] RectTransform  m_Content;

	IEnumerator m_RepositionRoutine;

	void Expand()
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
		
		m_RepositionRoutine = ExpandRoutine(m_Content, ShowDuration);
		
		StartCoroutine(m_RepositionRoutine);
	}

	void Shrink()
	{
		if (m_RepositionRoutine != null)
			StopCoroutine(m_RepositionRoutine);
		
		m_RepositionRoutine = ShrinkRoutine(m_Content, HideDuration);
		
		StartCoroutine(m_RepositionRoutine);
	}

	protected override IEnumerator ShowAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.alpha = 1;
		
		yield return ExpandRoutine(m_Content, _Duration);
	}

	protected override IEnumerator HideAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.alpha = 1;
		
		yield return ShrinkRoutine(m_Content, _Duration);
	}

	protected override void InstantShow(CanvasGroup _CanvasGroup)
	{
		base.InstantShow(_CanvasGroup);
		
		m_Content.anchorMin = Vector2.zero;
		m_Content.anchorMax = Vector2.one;
	}

	protected override void InstantHide(CanvasGroup _CanvasGroup)
	{
		base.InstantHide(_CanvasGroup);
		
		m_Content.anchorMin = new Vector2(0, -1);
		m_Content.anchorMax = new Vector2(1, 0);
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
		
		Vector2 min = m_Content.anchorMin;
		Vector2 max = m_Content.anchorMax;
		
		min.y = Mathf.Clamp(min.y + delta, -1, 0);
		max.y = Mathf.Clamp(max.y + delta, 0, 1);
		
		m_Content.anchorMin = min;
		m_Content.anchorMax = max;
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
		
		Vector2 anchor = m_Content.anchorMax;
		
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