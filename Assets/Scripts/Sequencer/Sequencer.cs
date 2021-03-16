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
		Initialize();
		
		if (!Application.isPlaying)
			return;
		
		Stop();
		Play();
	}

	void OnDestroy()
	{
		Stop();
	}

	void LateUpdate()
	{
		if (Playing)
			Sample(m_Time + UnityEngine.Time.deltaTime);
	}

	public void Initialize()
	{
		foreach (Track track in m_Tracks)
			track.Initialize(this);
	}

	public void Play()
	{
		Playing = true;
	}

	public void Pause()
	{
		Playing = false;
		
		Sample(Time);
	}

	public void Stop()
	{
		Time    = 0;
		Playing = false;
		
		Sample(Time);
	}

	public void Sample(float _Time)
	{
		foreach (Track track in m_Tracks)
			track.Sample(Time, _Time);
		
		Time = _Time;
	}
}