using System.Collections;
using UnityEngine;

public class UIShine : UIEntity
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

	[SerializeField, Range(0, 1)] float m_Phase;

	[SerializeField] RectTransform  m_Shine;
	[SerializeField] float          m_Duration    = 0.5f;
	[SerializeField] float          m_BeforeDelay = 2;
	[SerializeField] float          m_AfterDelay  = 8;
	[SerializeField] float          m_SourceWidth  = 300;
	[SerializeField] float          m_TargetWidth  = 600;
	[SerializeField] AnimationCurve m_Curve       = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[SerializeField] bool           m_Auto;
	[SerializeField] bool           m_Loop;

	IEnumerator m_PlayRoutine;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_Shine.gameObject.SetActive(false);
		
		Phase = 0;
		
		if (m_Auto)
			Play();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		m_Shine.gameObject.SetActive(false);
		
		Phase = 0;
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying || m_Shine == null)
			return;
		
		m_Shine.gameObject.SetActive(Phase > float.Epsilon && Phase < 1);
		
		ProcessPhase();
	}
	#endif

	public void Play()
	{
		if (m_PlayRoutine != null)
			StopCoroutine(m_PlayRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_PlayRoutine = PlayRoutine();
		
		StartCoroutine(m_PlayRoutine);
	}

	IEnumerator PlayRoutine()
	{
		while (m_Loop)
		{
			if (m_BeforeDelay > float.Epsilon)
				yield return new WaitForSeconds(m_BeforeDelay);
			
			m_Shine.gameObject.SetActive(true);
			
			Phase = 0;
			
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				Phase = m_Curve.Evaluate(time / m_Duration);
			}
			
			Phase = 1;
			
			m_Shine.gameObject.SetActive(false);
			
			if (m_AfterDelay > float.Epsilon)
				yield return new WaitForSeconds(m_AfterDelay);
		}
	}

	void ProcessPhase()
	{
		Vector2 sourcePivot = new Vector2(1, 0.5f);
		Vector2 targetPivot = new Vector2(0, 0.5f);
		
		Vector4 sourceAnchor = new Vector4(0, 0, 0, 1);
		Vector4 targetAnchor = new Vector4(1, 0, 1, 1);
		
		Vector4 anchor = Vector4.LerpUnclamped(sourceAnchor, targetAnchor, Phase);
		
		Vector2 size = m_Shine.sizeDelta;
		size.x = Mathf.LerpUnclamped(m_SourceWidth, m_TargetWidth, Phase);
		m_Shine.sizeDelta = size;
		
		m_Shine.pivot     = Vector2.LerpUnclamped(sourcePivot, targetPivot, Phase);
		m_Shine.anchorMin = new Vector2(anchor.x, anchor.y);
		m_Shine.anchorMax = new Vector2(anchor.z, anchor.w);
	}
}