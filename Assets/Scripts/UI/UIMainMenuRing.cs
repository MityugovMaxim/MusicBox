using System;
using System.Collections;
using UnityEngine;

public class UIMainMenuRing : UIEntity
{
	[SerializeField] float          m_Duration = 0.3f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] UISpline       m_Spline;
	[SerializeField] AnimationCurve m_Deformation = AnimationCurve.Linear(0, 0, 1, 1);

	IEnumerator m_MoveRoutine;
	Action      m_MoveFinished;

	public void Move(RectTransform _Target, bool _Instant = false, Action _Finished = null)
	{
		if (m_MoveRoutine != null)
			StopCoroutine(m_MoveRoutine);
		
		InvokeMoveFinished();
		
		m_MoveFinished = _Finished;
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_MoveRoutine = MoveRoutine(RectTransform, _Target);
			
			StartCoroutine(m_MoveRoutine);
		}
		else
		{
			Rect rect = _Target.GetWorldRect();
			
			RectTransform.position = rect.center;
			
			InvokeMoveFinished();
		}
	}

	IEnumerator MoveRoutine(RectTransform _Source, RectTransform _Target)
	{
		if (_Source == null || _Target == null)
			yield break;
		
		Rect rect = _Target.GetWorldRect();
		
		Vector2 source = _Source.position;
		Vector2 target = rect.center;
		
		int index = source.x < target.x ? 0 : 2;
		
		UISpline.Key sideKey   = m_Spline.GetKey(index);
		UISpline.Key topKey    = m_Spline.GetKey(1);
		UISpline.Key bottomKey = m_Spline.GetKey(3);
		
		float deformation = Mathf.Min(70, (source.x - target.x) * 0.15f);
		
		if (!Mathf.Approximately(source.x, target.x))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_Curve.Evaluate(time / m_Duration);
				
				sideKey.Position = Vector2.LerpUnclamped(
					new Vector2(0, 0),
					new Vector2(deformation, 0),
					m_Deformation.Evaluate(phase)
				);
				topKey.Position = new Vector2(0, -15) * Mathf.PingPong(phase * 2, 1);
				bottomKey.Position = new Vector2(0, 15) * Mathf.PingPong(phase * 2, 1);
				
				m_Spline.SetKey(index, sideKey);
				m_Spline.SetKey(1, topKey);
				m_Spline.SetKey(3, bottomKey);
				
				_Source.position = Vector2.Lerp(source, target, phase);
			}
		}
		
		sideKey.Position   = Vector2.zero;
		topKey.Position    = Vector2.zero;
		bottomKey.Position = Vector2.zero;
		m_Spline.SetKey(index, sideKey);
		m_Spline.SetKey(1, topKey);
		m_Spline.SetKey(3, bottomKey);
		
		_Source.position = target;
		
		InvokeMoveFinished();
	}

	void InvokeMoveFinished()
	{
		Action action = m_MoveFinished;
		m_MoveFinished = null;
		action?.Invoke();
	}
}