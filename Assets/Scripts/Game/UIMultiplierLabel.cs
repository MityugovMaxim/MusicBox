using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class UIMultiplierLabel : UIOrder
{
	const string PLAY_STATE = "play";

	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_PlayParameterID    = Animator.StringToHash("Play");

	public int Multiplier
	{
		get => m_Multiplier;
		set
		{
			if (m_Multiplier == value)
				return;
			
			m_Multiplier = value;
			
			ProcessMultiplier();
		}
	}

	[SerializeField] UIUnitLabel[] m_Labels;
	[SerializeField] Vector2       m_Offset;
	[SerializeField] int           m_Multiplier;
	[SerializeField] float         m_Length;
	[SerializeField] float         m_Scale;
	[SerializeField] float         m_Rotation;

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessMultiplier();
		ProcessLength();
		ProcessScale();
		ProcessRotation();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessLength();
		ProcessScale();
		ProcessRotation();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessMultiplier();
		ProcessLength();
		ProcessScale();
		ProcessRotation();
	}
	#endif

	public void Restore()
	{
		m_Animator.SetTrigger(m_RestoreParameterID);
	}

	public void Play()
	{
		Restore();
		
		m_Offset = Random.rotation * Vector2.up;
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}

	void ProcessMultiplier()
	{
		foreach (UIUnitLabel label in m_Labels)
			label.Value = m_Multiplier;
	}

	void ProcessLength()
	{
		float step = 1.0f / (m_Labels.Length - 1);
		for (int i = 0; i < m_Labels.Length; i++)
			m_Labels[i].RectTransform.anchoredPosition = m_Offset * m_Length * step * i;
	}

	void ProcessScale()
	{
		float step = 1.0f / (m_Labels.Length - 1);
		for (int i = 0; i < m_Labels.Length; i++)
			m_Labels[i].RectTransform.localScale = Vector3.one * (1 + m_Scale * step * i);
	}

	void ProcessRotation()
	{
		float step = 1.0f / (m_Labels.Length - 1);
		for (int i = 0; i < m_Labels.Length; i++)
			m_Labels[i].RectTransform.localEulerAngles = new Vector3(0, 0, m_Rotation * step * i);
	}
}