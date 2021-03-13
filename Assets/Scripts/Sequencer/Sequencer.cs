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
		Time    = 0;
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
		
		if (!Application.isPlaying)
			AudioUtility.StopAllClips();
		
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