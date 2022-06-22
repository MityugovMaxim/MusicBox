using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimatorSpectrum : UISpectrum
{
	static readonly int m_PlayParameterID = Animator.StringToHash("Play");

	[SerializeField, Range(0, 1)] float m_Threshold = 0.5f;
	[SerializeField]              int   m_Channel;
	[SerializeField]              float m_Duration = 0.1f;

	Animator m_Animator;
	int      m_TriggerID;
	bool     m_Active;
	float    m_Time;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		if (Time.time < m_Time)
			return;
		
		int channel = Mathf.Clamp(m_Channel, 0, _Amplitude.Length - 1);
		
		float amplitude = _Amplitude[channel];
		
		if (amplitude < m_Threshold)
		{
			m_Active = false;
			return;
		}
		
		if (m_Active)
			return;
		
		m_Time = Time.time + m_Duration;
		
		m_Animator.SetTrigger(m_PlayParameterID);
	}
}