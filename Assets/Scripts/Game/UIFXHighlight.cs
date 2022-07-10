using System.Collections;
using UnityEngine;

public class UIFXHighlight : UIOrder
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

	[SerializeField] UISprite       m_Graphic;
	[SerializeField] float          m_Duration;
	[SerializeField] float          m_SourceAlpha = 1;
	[SerializeField] float          m_TargetAlpha = 0;
	[SerializeField] Vector3        m_SourceScale;
	[SerializeField] Vector3        m_TargetScale;
	[SerializeField] AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

	IEnumerator m_PlayRoutine;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced)
			return;
		
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
		m_Graphic.gameObject.SetActive(true);
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			Phase = time / m_Duration;
		}
		
		m_Graphic.gameObject.SetActive(false);
	}

	void ProcessPhase()
	{
		float phase = m_Curve.Evaluate(Phase);
		
		float   alpha = Mathf.Lerp(m_SourceAlpha, m_TargetAlpha, phase);
		Vector3 scale = Vector4.Lerp(m_SourceScale, m_TargetScale, phase);
		
		m_Graphic.Alpha                    = alpha;
		m_Graphic.RectTransform.localScale = scale;
	}
}