using UnityEngine;

[ExecuteAlways]
public class UIPath : UIEntity
{
	public float Phase
	{
		get => m_Phase;
		set
		{
			if (Mathf.Approximately(m_Phase, value))
				return;
			
			m_Phase = value;
			
			ProcessPosition();
		}
	}

	public RectTransform Source
	{
		get => m_Source;
		set
		{
			if (m_Source == value)
				return;
			
			m_Source = value;
			
			ProcessPosition();
		}
	}

	public RectTransform Target
	{
		get => m_Target;
		set
		{
			if (m_Target == value)
				return;
			
			m_Target = value;
			
			ProcessPosition();
		}
	}

	static readonly Vector2 m_Center = new Vector2(0.5f, 0.5f);

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] RectTransform m_Source;
	[SerializeField] RectTransform m_Target;
	[SerializeField] Vector2       m_SourceTangent;
	[SerializeField] Vector2       m_TargetTangent;

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessPosition();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying || !IsInstanced)
			return;
		
		ProcessPosition();
	}
	#endif

	public void Setup(RectTransform _Source, RectTransform _Target)
	{
		m_Source = _Source;
		m_Target = _Target;
		m_Phase  = 0;
		
		ProcessPosition();
	}

	void ProcessPosition()
	{
		if (Source == null || Target == null)
			return;
		
		Rect sourceRect = Source.GetWorldRect();
		Rect targetRect = Target.GetWorldRect();
		
		Vector2 sourcePivot = Source.pivot;
		Vector2 targetPivot = Target.pivot;
		
		Vector2 sourceSize = sourceRect.size;
		Vector2 targetSize = targetRect.size;
		
		Vector2 sourceTangent = Vector2.Scale(sourceSize, m_SourceTangent);
		Vector2 targetTangent = Vector2.Scale(targetSize, m_TargetTangent);
		
		Vector2 sourcePosition = sourceRect.position + Vector2.Scale(sourceSize, sourcePivot);
		Vector2 targetPosition = targetRect.position + Vector2.Scale(targetSize, targetPivot);
		Vector2 position       = Bezier(sourcePosition, targetPosition, sourceTangent, targetTangent, Phase);
		
		sourceSize = RectTransform.InverseTransformVector(sourceSize);
		targetSize = RectTransform.InverseTransformVector(targetSize);
		
		RectTransform.anchorMin = m_Center;
		RectTransform.anchorMax = m_Center;
		RectTransform.position  = position;
		RectTransform.sizeDelta = Vector2.Lerp(sourceSize, targetSize, Phase);
		RectTransform.pivot     = Vector2.Lerp(sourcePivot, targetPivot, Phase);
	}

	static Vector2 Bezier(
		Vector2 _SourcePosition,
		Vector2 _TargetPosition,
		Vector2 _SourceTangent,
		Vector2 _TargetTangent,
		float   _Phase
	)
	{
		Vector2 a = _SourcePosition;
		Vector2 b = _SourcePosition + _SourceTangent;
		Vector2 c = _TargetPosition + _TargetTangent;
		Vector2 d = _TargetPosition;
		
		Vector2 e = Vector2.Lerp(a, b, _Phase);
		Vector2 f = Vector2.Lerp(b, c, _Phase);
		Vector2 g = Vector2.Lerp(c, d, _Phase);
		
		Vector2 h = Vector2.Lerp(e, f, _Phase);
		Vector2 i = Vector2.Lerp(f, g, _Phase);
		
		return Vector2.Lerp(h, i, _Phase);
	}
}
