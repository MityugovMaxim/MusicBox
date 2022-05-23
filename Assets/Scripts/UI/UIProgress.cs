using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
public class UIProgress : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

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

	float Weight => 1;

	[SerializeField, Range(0, 1)] float          m_Progress;
	[SerializeField, Range(0, 1)] float          m_Falloff;
	[SerializeField]              AnimationCurve m_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField]              Gradient       m_Gradient;
	[SerializeField]              float          m_MinHeight;
	[SerializeField]              float          m_MaxHeight;
	[SerializeField]              Image[]        m_Dots;

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_Animator.SetTrigger(m_RestoreParameterID);
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
		float step = 1.0f / Mathf.Max(1, m_Dots.Length + 1);
		
		float min = Progress - m_Falloff;
		float max = Progress + m_Falloff * Progress;
		
		for (int i = 0; i < m_Dots.Length; i++)
		{
			Image dot = m_Dots[i];
			
			float position = step * i;
			
			float phase = m_Curve.Evaluate(MathUtility.Remap01(position, min, max)) * Weight;
			
			Vector2 size = dot.rectTransform.sizeDelta;
			
			size.y = Mathf.Lerp(m_MinHeight, m_MaxHeight, phase);
			
			dot.rectTransform.sizeDelta = size;
			
			dot.color = m_Gradient.Evaluate(phase);
		}
	}

	[ContextMenu("Reverse")]
	public void Reverse()
	{
		for (int i = 0; i < m_Dots.Length; i++)
		{
			int j = (m_Dots.Length - i) % m_Dots.Length;
			
			(m_Dots[i], m_Dots[j]) = (m_Dots[j], m_Dots[i]);
		}
	}

	[ContextMenu("Generate")]
	public void Generate()
	{
		Color   sourceColor = new Color(1, 1, 1);
		Color   targetColor = new Color(0.25f, 0.75f, 1f);
		Vector2 sourceSize  = new Vector2(20, 20);
		Vector2 targetSize  = new Vector2(20, 50);
		Image[] images      = GetComponentsInChildren<Image>();
		foreach (Image image in images)
		{
			float phase = Random.value;
			
			image.rectTransform.sizeDelta = Vector2.Lerp(sourceSize, targetSize, phase);
			
			image.color = Color.Lerp(sourceColor, targetColor, phase);
		}
	}
}