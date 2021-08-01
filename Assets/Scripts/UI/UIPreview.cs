using System;
using System.Collections;
using UnityEngine;

public class UIPreview : UIEntity
{
	[SerializeField] RectTransform  m_Content;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	bool m_Shown;

	Thumbnail   m_Thumbnail;
	IEnumerator m_MoveRoutine;

	public void Show(Thumbnail _Thumbnail, bool _Instant = false)
	{
		if (m_Thumbnail != _Thumbnail)
		{
			if (m_Thumbnail != null)
			{
				m_Thumbnail.RectTransform.SetParent(m_Thumbnail.Mount);
				m_Thumbnail.RectTransform.anchorMin = Vector2.zero;
				m_Thumbnail.RectTransform.anchorMax = Vector2.one;
				m_Thumbnail.RectTransform.offsetMin = Vector2.zero;
				m_Thumbnail.RectTransform.offsetMax = Vector2.zero;
				m_Thumbnail.OnHide();
			}
			
			m_Thumbnail = _Thumbnail;
			
			m_Thumbnail.RectTransform.SetParent(m_Content, true);
		}
		
		Rect source = m_Content.GetWorldRect();
		Rect target = m_Thumbnail.GetWorldRect();
		
		Vector2 anchorMin = new Vector2(
			(target.xMin - source.xMin) / source.width,
			(target.yMin - source.yMin) / source.height
		);
		
		Vector2 anchorMax = new Vector2(
			(target.xMax - source.xMin) / source.width,
			(target.yMax - source.yMin) / source.height
		);
		
		m_Thumbnail.RectTransform.anchorMin  = anchorMin;
		m_Thumbnail.RectTransform.anchorMax  = anchorMax;
		m_Thumbnail.RectTransform.offsetMin  = Vector2.zero;
		m_Thumbnail.RectTransform.offsetMax  = Vector2.zero;
		m_Thumbnail.RectTransform.localScale = Vector3.one;
		
		m_Thumbnail.OnShow();
		
		Move(m_Thumbnail, Vector2.zero, Vector2.one, _Instant || m_Shown);
		
		m_Shown = true;
	}

	public void Hide(bool _Instant = false)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		Rect source = m_Content.GetWorldRect();
		Rect target = m_Thumbnail.Mount.GetWorldRect();
		
		Vector2 anchorMin = new Vector2(
			(target.xMin - source.xMin) / source.width,
			(target.yMin - source.yMin) / source.height
		);
		
		Vector2 anchorMax = new Vector2(
			(target.xMax - source.xMin) / source.width,
			(target.yMax - source.yMin) / source.height
		);
		
		m_Thumbnail.OnHide();
		
		Move(
			m_Thumbnail,
			anchorMin,
			anchorMax,
			_Instant,
			() =>
			{
				m_Thumbnail.RectTransform.SetParent(m_Thumbnail.Mount, true);
				m_Thumbnail.RectTransform.anchorMin  = Vector2.zero;
				m_Thumbnail.RectTransform.anchorMax  = Vector2.one;
				m_Thumbnail.RectTransform.offsetMin  = Vector2.zero;
				m_Thumbnail.RectTransform.offsetMax  = Vector2.zero;
				m_Thumbnail.RectTransform.localScale = Vector3.one;
				
				m_Thumbnail = null;
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