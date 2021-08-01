using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public partial class Sequencer
{
	public float MinTime
	{
		get => m_MinTime;
		set => m_MinTime = value;
	}

	public float MaxTime
	{
		get => m_MaxTime;
		set => m_MaxTime = value;
	}

	public float Length
	{
		get => m_Length;
		set => m_Length = value;
	}

	public float TracksWidth
	{
		get => m_TracksWidth;
		set => m_TracksWidth = value;
	}

	public float ScrollPosition
	{
		get => m_ScrollPosition;
		set => m_ScrollPosition = value;
	}

	[SerializeField] float m_MinTime        = 0;
	[SerializeField] float m_MaxTime        = 60;
	[SerializeField] float m_TracksWidth    = 120;
	[SerializeField] float m_ScrollPosition = 0;
}

[ExecuteInEditMode]
public partial class Sequencer : MonoBehaviour
{
	[Serializable]
	public class SampleEvent : UnityEvent<float, float> { }

	public float Time
	{
		get => m_Time;
		private set => m_Time = value;
	}

	public bool Playing { get; private set; }

	public Track[] Tracks => m_Tracks;

	[SerializeField] Track[] m_Tracks;
	[SerializeField] float   m_Time;
	[SerializeField] float   m_Length;

	ISampleReceiver[] m_SampleReceivers;
	Action            m_Finished;
	int               m_Frame;
	int               m_SampleFrame;

	void OnDisable()
	{
		if (!Application.isPlaying)
			return;
		
		Pause();
	}

	void LateUpdate()
	{
		if (Playing && m_SampleFrame != m_Frame)
		{
			m_SampleFrame = m_Frame;
			
			Sample(m_Time + UnityEngine.Time.deltaTime);
		}
		
		m_Frame++;
	}

	[Inject]
	public void Construct(ISampleReceiver[] _SampleReceivers)
	{
		m_SampleReceivers = _SampleReceivers;
	}

	public void Initialize()
	{
		if (m_Tracks == null || m_Tracks.Length == 0)
			return;
		
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	public void Play(Action _Finished = null)
	{
		Playing = true;
		
		m_Finished    = _Finished;
		m_Frame       = 0;
		m_SampleFrame = 0;
		
		Sample(Time);
	}

	public void Pause()
	{
		Playing = false;
		
		m_Frame       = 0;
		m_SampleFrame = 0;
		
		Sample(Time);
	}

	public void Stop()
	{
		Playing = false;
		
		m_Frame       = 0;
		m_SampleFrame = 0;
		
		Sample(0);
	}

	public void Sample(float _Time)
	{
		foreach (Track track in m_Tracks)
			track.Sample(Time, _Time);
		
		Time = _Time;
		
		if (m_SampleReceivers != null)
		{
			foreach (var sampleReceiver in m_SampleReceivers)
				sampleReceiver.Sample(Time, Length);
		}
		
		if (Time >= m_Length)
		{
			Playing = false;
			
			foreach (Track track in m_Tracks)
				track.Sample(Time, m_Length);
			
			Time = m_Length;
			
			m_Finished?.Invoke();
		}
	}
}