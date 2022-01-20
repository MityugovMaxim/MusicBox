using UnityEngine;

[ExecuteInEditMode]
public class UILoaderElement : UIEntity
{
	[SerializeField] float      m_Phase;
	[SerializeField] UICircle[] m_Circles;
	[SerializeField] float      m_Distance;
	[SerializeField] Gradient   m_Gradient;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Reposition();
		
		ProcessPhase();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		Reposition();
		
		ProcessPhase();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		Reposition();
		
		ProcessPhase();
	}
	#endif

	void Reposition()
	{
		float   step      = 1.0f / m_Circles.Length;
		Vector2 direction = Vector2.up * m_Distance;
		for (int i = 0; i < m_Circles.Length; i++)
			m_Circles[i].rectTransform.anchoredPosition = Quaternion.Euler(0, 0, 360.0f * step * i) *direction;
	}

	void ProcessPhase()
	{
		foreach (UICircle circle in m_Circles)
		{
			circle.Size  = Mathf.Lerp(0.01f, 1, m_Phase);
			circle.color = m_Gradient.Evaluate(m_Phase);
		}
	}
}