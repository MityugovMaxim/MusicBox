using UnityEngine;

[ExecuteAlways]
public class Sequencer : MonoBehaviour
{
	public float Time
	{
		get => m_Time;
		set => m_Time = value;
	}

	public bool Playing { get; private set; }

	public Track[] Tracks => m_Tracks;

	[SerializeField] Track[] m_Tracks;

	[SerializeField] float m_Time;

	void Awake()
	{
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	void LateUpdate()
	{
		if (Playing)
			Sample(m_Time + UnityEngine.Time.deltaTime);
	}

	public void Play()
	{
		Playing = true;
	}

	public void Pause()
	{
		Playing = false;
	}

	public void Stop()
	{
		foreach (Track track in m_Tracks)
			track.Stop(m_Time);
		
		Playing = false;
		Time    = 0;
	}

	public void Sample(float _Time)
	{
		if (Mathf.Approximately(Time, _Time))
			return;
		
		foreach (Track track in m_Tracks)
			track.Sample(Time, _Time);
		
		Time = _Time;
	}
}