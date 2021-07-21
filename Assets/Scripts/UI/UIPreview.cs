using System;
using System.Collections;
using UnityEngine;

public class UIPreview : UIEntity
{
	[SerializeField] RectTransform  m_Content;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	bool m_Shown;

	Preview       m_Preview;
	RectTransform m_Mount;
	IEnumerator   m_MoveRoutine;

	public void Show(Preview _Preview)
	{
		if (m_Preview != _Preview)
		{
			if (m_Preview != null)
			{
				m_Preview.RectTransform.SetParent(m_Mount);
				m_Preview.RectTransform.anchorMin = Vector2.zero;
				m_Preview.RectTransform.anchorMax = Vector2.one;
				m_Preview.RectTransform.offsetMin = Vector2.zero;
				m_Preview.RectTransform.offsetMax = Vector2.zero;
			}
			
			m_Preview = _Preview;
			m_Mount   = m_Preview.RectTransform.parent as RectTransform;
			
			m_Preview.RectTransform.SetParent(m_Content, true);
		}
		
		Rect source = m_Content.GetWorldRect();
		Rect target = m_Preview.GetWorldRect();
		
		Vector2 anchorMin = new Vector2(
			(target.xMin - source.xMin) / source.width,
			(target.yMin - source.yMin) / source.height
		);
		
		Vector2 anchorMax = new Vector2(
			(target.xMax - source.xMin) / source.width,
			(target.yMax - source.yMin) / source.height
		);
		
		m_Preview.RectTransform.anchorMin = anchorMin;
		m_Preview.RectTransform.anchorMax = anchorMax;
		m_Preview.RectTransform.offsetMin = Vector2.zero;
		m_Preview.RectTransform.offsetMax = Vector2.zero;
		
		Move(_Preview, Vector2.zero, Vector2.one, m_Shown);
		
		m_Shown = true;
	}

	public void Hide(bool _Instant = false)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		Rect source = m_Content.GetWorldRect();
		Rect target = m_Mount.GetWorldRect();
		
		Vector2 anchorMin = new Vector2(
			(target.xMin - source.xMin) / source.width,
			(target.yMin - source.yMin) / source.height
		);
		
		Vector2 anchorMax = new Vector2(
			(target.xMax - source.xMin) / source.width,
			(target.yMax - source.yMin) / source.height
		);
		
		Move(
			m_Preview,
			anchorMin,
			anchorMax,
			_Instant,
			() =>
			{
				m_Preview.RectTransform.SetParent(m_Mount, true);
				m_Preview.RectTransform.anchorMin = Vector2.zero;
				m_Preview.RectTransform.anchorMax = Vector2.one;
				m_Preview.RectTransform.offsetMin = Vector2.zero;
				m_Preview.RectTransform.offsetMax = Vector2.zero;
				
				m_Preview = null;
				m_Mount   = null;
			}
		);
	}

	void Move(UIEntity _Preview, Vector2 _AnchorMin, Vector2 _AnchorMax, bool _Instant = false, Action _Finished = null)
	{
		if (m_MoveRoutine != null)
			StopCoroutine(m_MoveRoutine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			_Preview.RectTransform.anchorMin = _AnchorMin;
			_Preview.RectTransform.anchorMax = _AnchorMax;
			_Finished?.Invoke();
		}
		else
		{
			m_MoveRoutine = MoveRoutine(
				_Preview.RectTransform,
				_AnchorMin,
				_AnchorMax,
				m_Duration,
				m_Curve,
				_Finished
			);
			StartCoroutine(m_MoveRoutine);
		}
	}

	static IEnumerator MoveRoutine(RectTransform _Target, Vector2 _AnchorMin, Vector2 _AnchorMax, float _Duration, AnimationCurve _Curve, Action _Finished)
	{
		if (_Target == null)
			yield break;
		
		Vector2 sourceMin = _Target.anchorMin;
		Vector2 sourceMax = _Target.anchorMax;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = _Curve.Evaluate(time / _Duration);
			
			_Target.anchorMin = Vector2.Lerp(sourceMin, _AnchorMin, phase);
			_Target.anchorMax = Vector2.Lerp(sourceMax, _AnchorMax, phase);
		}
		
		_Target.anchorMin = _AnchorMin;
		_Target.anchorMax = _AnchorMax;
		
		_Finished?.Invoke();
	}
}