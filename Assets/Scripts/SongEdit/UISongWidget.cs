using System.Collections;
using UnityEngine;

public class UISongWidget : UIEntity
{
	[SerializeField] float m_SourceScale = 0.3f;
	[SerializeField] float m_TargetScale = 1;
	[SerializeField] float m_Duration    = 0.2f;
	[SerializeField] float m_Cooldown    = 5;

	bool m_Expanded;

	IEnumerator m_ExpandRoutine;

	protected bool Expand()
	{
		bool expanded = m_Expanded;
		
		if (m_ExpandRoutine != null)
			StopCoroutine(m_ExpandRoutine);
		
		m_ExpandRoutine = ExpandRoutine();
		
		StartCoroutine(m_ExpandRoutine);
		
		return !expanded;
	}

	IEnumerator ExpandRoutine()
	{
		Vector3 scale = RectTransform.localScale;
		
		float source = Mathf.Min(scale.x, scale.y);
		float target = m_TargetScale;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float value = EaseFunction.EaseOutQuad.Get(source, target, time / m_Duration);
				
				scale.x = value;
				scale.y = value;
				
				RectTransform.localScale = scale;
			}
		}
		
		m_Expanded = true;
		
		scale.x = target;
		scale.y = target;
		
		RectTransform.localScale = scale;
		
		yield return new WaitForSeconds(m_Cooldown);
		
		source = Mathf.Min(scale.x, scale.y);
		target = m_SourceScale;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				float value = EaseFunction.EaseOutQuad.Get(source, target, time / m_Duration);
				
				scale.x = value;
				scale.y = value;
				
				RectTransform.localScale = scale;
			}
		}
		
		m_Expanded = false;
	}
}
