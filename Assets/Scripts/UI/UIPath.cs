using UnityEngine;

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
			
			ProcessPhase();
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
			
			ProcessPhase();
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
			
			ProcessPhase();
		}
	}

	static readonly Vector2 m_Anchor = new Vector2(0.5f, 0.5f);

	[SerializeField, Range(0, 1)] float         m_Phase;
	[SerializeField]              RectTransform m_Source;
	[SerializeField]              RectTransform m_Target;
	[SerializeField]              RectTransform m_Object;

	#if UNITY_EDITOR
	DrivenRectTransformTracker m_Tracker;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_Tracker = new DrivenRectTransformTracker();
		
		ProcessPhase();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_Tracker.Clear();
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced)
			return;
		
		m_Tracker.Clear();
		m_Tracker.Add(
			this,
			m_Object,
			DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax | DrivenTransformProperties.SizeDelta
		);
		
		ProcessPhase();
	}
	#endif

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessPhase();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		
		ProcessPhase();
	}

	void ProcessPhase()
	{
		if (!gameObject.activeInHierarchy || m_Source == null || m_Target == null || m_Object == null)
			return;
		
		Rect source = m_Source.GetWorldRect();
		Rect target = m_Target.GetWorldRect();
		Rect rect = new Rect(
			Vector2.Lerp(source.position, target.position, Phase),
			Vector2.Lerp(source.size, target.size, Phase)
		);
		
		rect = RectTransform.InverseTransformRect(rect);
		
		m_Object.anchorMin        = m_Anchor;
		m_Object.anchorMax        = m_Anchor;
		m_Object.anchoredPosition = rect.position + Vector2.Scale(rect.size, m_Object.pivot);
		m_Object.sizeDelta        = rect.size;
	}
}