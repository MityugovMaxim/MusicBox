using UnityEngine;

[ExecuteInEditMode]
public class UISplinePath : UIEntity
{
	public float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPhase();
		}
	}

	public UISpline Spline
	{
		get => m_Spline;
		set
		{
			if (m_Spline == value)
				return;
			
			m_Spline = value;
			
			ProcessPhase();
		}
	}

	[SerializeField, Range(0, 1)] float          m_Phase; 
	[SerializeField]              UISpline       m_Spline;
	[SerializeField]              Vector2        m_SourceSize;
	[SerializeField]              Vector2        m_TargetSize;
	[SerializeField]              AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessPhase();
		
		m_Spline.OnRebuild += ProcessPhase;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Spline.OnRebuild -= ProcessPhase;
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessPhase();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessPhase();
	}
	#endif

	void ProcessPhase()
	{
		UISpline.Point point = m_Spline.GetPoint(Phase);
		
		RectTransform.anchoredPosition = point.Position;
		RectTransform.sizeDelta        = Vector2.LerpUnclamped(m_SourceSize, m_TargetSize, m_Curve.Evaluate(Phase));
	}
}
