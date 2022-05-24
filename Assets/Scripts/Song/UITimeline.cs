using UnityEngine;

[ExecuteInEditMode]
public class UITimeline : UIEntity, IASFSampler
{
	public float Progress
	{
		get => m_Progress;
		set
		{
			if (Mathf.Approximately(m_Progress, value))
				return;
			
			m_Progress = value;
			
			ProcessProgress();
		}
	}

	[SerializeField, Range(0, 1)] float m_Progress;

	[SerializeField] RectTransform m_Target;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessProgress();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessProgress();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessProgress();
	}
	#endif

	void ProcessProgress()
	{
		Vector2 anchorMax = m_Target.anchorMax;
		
		anchorMax.x = Progress;
		
		m_Target.anchorMax = anchorMax;
	}

	void IASFSampler.Sample(double _Time, double _Length)
	{
		Progress = Mathf.Clamp01((float)(_Time / _Length));
	}
}