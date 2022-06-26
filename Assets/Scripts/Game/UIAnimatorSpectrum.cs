using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class UIAnimatorSpectrum : UISpectrum
{
	[Serializable]
	public class Trigger
	{
		public float Duration => m_Duration;

		[SerializeField] string m_Parameter;
		[SerializeField] float  m_Duration;

		int m_Hash;

		public void Initialize()
		{
			m_Hash = Animator.StringToHash(m_Parameter);
		}

		public void Process(Animator _Animator)
		{
			_Animator.SetTrigger(m_Hash);
		}
	}

	[SerializeField, Range(0, 1)]    float     m_Threshold;
	[SerializeField]                 int       m_Channel;
	[SerializeField, NonReorderable] Trigger[] m_Triggers;

	Animator m_Animator;
	float    m_Time;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		foreach (Trigger trigger in m_Triggers)
			trigger.Initialize();
	}

	public override void Reposition() { }

	public override void Sample(float[] _Amplitude)
	{
		if (Time.time < m_Time)
			return;
		
		int channel = Mathf.Clamp(m_Channel, 0, _Amplitude.Length - 1);
		
		float amplitude = _Amplitude[channel];
		
		if (amplitude < m_Threshold)
			return;
		
		int index = Random.Range(0, m_Triggers.Length);
		
		Trigger trigger = m_Triggers[index];
		
		m_Time = Time.time + trigger.Duration;
		
		trigger.Process(m_Animator);
	}
}