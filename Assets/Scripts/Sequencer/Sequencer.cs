using UnityEngine;

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

	public float TracksWidth
	{
		get => m_TracksWidth;
		set => m_TracksWidth = value;
	}

	[SerializeField] float m_MinTime     = 0;
	[SerializeField] float m_MaxTime     = 60;
	[SerializeField] float m_TracksWidth = 120;
}

[ExecuteInEditMode]
public partial class Sequencer : MonoBehaviour
{
	public float Time
	{
		get => m_Time;
		private set => m_Time = value;
	}

	public bool Playing { get; private set; }

	public Track[] Tracks => m_Tracks;

	[SerializeField] Track[] m_Tracks;
	[SerializeField] float   m_Time;
	[SerializeField] bool    m_AutoPlay;

	int m_Frame;
	int m_SampleFrame;

	void OnEnable()
	{
		Initialize();
		
		if (!Application.isPlaying || !m_AutoPlay)
			return;
		
		Stop();
		Play();
	}

	void OnDisable()
	{
		Stop();
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

	public void Initialize()
	{
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	public void Play()
	{
		Playing = true;
		
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
	}
}