using System.Collections;
using UnityEngine;

public class UIMainMenuRing : UIEntity
{
	[SerializeField] float          m_Duration = 0.3f;
	[SerializeField] AnimationCurve m_Curve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	IEnumerator m_MoveRoutine;

	public void Move(RectTransform _Target, bool _Instant = false)
	{
		if (m_MoveRoutine != null)
			StopCoroutine(m_MoveRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_MoveRoutine = MoveRoutine(RectTransform, _Target);
			
			StartCoroutine(m_MoveRoutine);
		}
		else
		{
			Rect rect = _Target.GetWorldRect();
			
			RectTransform.position = rect.center;
		}
	}

	IEnumerator MoveRoutine(RectTransform _Source, RectTransform _Target)
	{
		if (_Source == null || _Target == null)
			yield break;
		
		Rect rect = _Target.GetWorldRect();
		
		Vector2 source = _Source.position;
		Vector2 target = rect.center;
		
		if (source != target)
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Source.position = Vector2.Lerp(source, target, m_Curve.Evaluate(time / m_Duration));
			}
		}
		
		_Source.position = target;
	}
}