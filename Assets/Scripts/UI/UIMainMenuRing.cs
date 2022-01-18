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

	IEnumerator RelaxRoutine(float _Duration)
	{
		UISpline.Key leftKey   = m_Spline.GetKey(0);
		UISpline.Key topKey    = m_Spline.GetKey(1);
		UISpline.Key rightKey  = m_Spline.GetKey(2);
		UISpline.Key bottomKey = m_Spline.GetKey(3);
		
		Vector2 leftSource   = leftKey.Position;
		Vector2 rightSource  = rightKey.Position;
		Vector2 topSource    = topKey.Position;
		Vector2 bottomSource = bottomKey.Position;
		
		float time = 0;
		while (time < _Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = time / _Duration;
			
			leftKey.Position   = Vector2.LerpUnclamped(leftSource, Vector2.zero, phase);
			rightKey.Position  = Vector2.LerpUnclamped(rightSource, Vector2.zero, phase);
			topKey.Position    = Vector2.LerpUnclamped(topSource, Vector2.zero, phase);
			bottomKey.Position = Vector2.LerpUnclamped(bottomSource, Vector2.zero, phase);
			
			m_Spline.SetKey(0, leftKey);
			m_Spline.SetKey(1, topKey);
			m_Spline.SetKey(2, rightKey);
			m_Spline.SetKey(3, bottomKey);
		}
		
		leftKey.Position   = Vector2.zero;
		rightKey.Position  = Vector2.zero;
		topKey.Position    = Vector2.zero;
		bottomKey.Position = Vector2.zero;
		
		m_Spline.SetKey(0, leftKey);
		m_Spline.SetKey(1, topKey);
		m_Spline.SetKey(2, rightKey);
		m_Spline.SetKey(3, bottomKey);
	}

	IEnumerator MoveRoutine(RectTransform _Source, RectTransform _Target)
	{
		if (_Source == null || _Target == null)
			yield break;
		
		UISpline.Key leftKey   = m_Spline.GetKey(0);
		UISpline.Key topKey    = m_Spline.GetKey(1);
		UISpline.Key rightKey  = m_Spline.GetKey(2);
		UISpline.Key bottomKey = m_Spline.GetKey(3);
		
		if (leftKey.Position != Vector2.zero || rightKey.Position != Vector2.zero || topKey.Position != Vector2.zero || bottomKey.Position != Vector2.zero)
			yield return RelaxRoutine(0.1f);
		
		Rect rect = _Target.GetWorldRect();
		
		Vector2 source = _Source.position;
		Vector2 target = rect.center;
		
		float distance = (source.x - target.x) * 0.15f;
		
		Vector2 leftTarget   = distance < 0 ? new Vector2(Mathf.Clamp(distance, -70, 0), 0) : Vector2.zero;
		Vector2 topTarget    = new Vector2(0, -15);
		Vector2 rightTarget  = distance > 0 ? new Vector2(Mathf.Clamp(distance, 0, 70), 0) : Vector2.zero;
		Vector2 bottomTarget = new Vector2(0, 15);
		
		if (!Mathf.Approximately(source.x, target.x))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float phase = m_Curve.Evaluate(time / m_Duration);
				
				float deformation = m_Deformation.Evaluate(phase);
				
				leftKey.Position   = Vector2.LerpUnclamped(Vector2.zero, leftTarget, deformation);
				topKey.Position    = Vector2.LerpUnclamped(Vector2.zero, topTarget, Mathf.PingPong(phase * 2, 1));
				rightKey.Position  = Vector2.LerpUnclamped(Vector2.zero, rightTarget, deformation);
				bottomKey.Position = Vector2.LerpUnclamped(Vector2.zero, bottomTarget, Mathf.PingPong(phase * 2, 1));
				
				m_Spline.SetKey(0, leftKey);
				m_Spline.SetKey(1, topKey);
				m_Spline.SetKey(2, rightKey);
				m_Spline.SetKey(3, bottomKey);
				
				_Source.position = Vector2.Lerp(source, target, phase);
			}
		}
		
		leftKey.Position   = Vector2.zero;
		rightKey.Position  = Vector2.zero;
		topKey.Position    = Vector2.zero;
		bottomKey.Position = Vector2.zero;
		m_Spline.SetKey(0, leftKey);
		m_Spline.SetKey(1, topKey);
		m_Spline.SetKey(2, rightKey);
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