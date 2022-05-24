using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
public class UILoader : UIEntity
{
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");

	[SerializeField]              float          m_Progress;
	[SerializeField, Range(0, 1)] float          m_Size;
	[SerializeField, Range(0, 1)] float          m_Weight;
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
		
		ProcessLoader();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessLoader();
	}
	#endif

	void ProcessLoader()
	{
		float step = 1.0f / Mathf.Max(1, m_Dots.Length);
		
		float progress = Mathf.Repeat(m_Progress, 1);
		float min      = progress - m_Size;
		float max      = progress + m_Size;
		
		for (int i = 0; i < m_Dots.Length; i++)
		{
			Image dot = m_Dots[i];
			
			float position = step * i;
			
			float phase = 0;
			phase += m_Curve.Evaluate(MathUtility.Remap01(position, min, max));
			phase += m_Curve.Evaluate(MathUtility.Remap01(position, min + 1, max + 1));
			phase += m_Curve.Evaluate(MathUtility.Remap01(position, min - 1, max - 1));
			phase *= m_Weight;
			
			Vector2 size = dot.rectTransform.sizeDelta;
			
			size.y = Mathf.Lerp(m_MinHeight, m_MaxHeight, phase);
			
			dot.rectTransform.sizeDelta = size;
			
			dot.color = m_Gradient.Evaluate(phase);
		}
	}
}