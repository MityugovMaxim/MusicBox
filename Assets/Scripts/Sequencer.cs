using UnityEngine;

public class Sequencer : MonoBehaviour
{
	public Track[] Tracks => m_Tracks;

	[SerializeField] Track[] m_Tracks;

	float m_Time;
	bool  m_Playing;

	void LateUpdate()
	{
		if (m_Playing)
			Sample(m_Time + Time.deltaTime);
	}

	public void Play()
	{
		m_Playing = true;
	}

	public void Pause()
	{
		m_Playing = false;
	}

	public void Stop()
	{
		m_Playing = false;
		m_Time    = 0;
	}

	public void Sample(float _Time)
	{
		float startTime  = m_Time;
		float finishTime = _Time;
		
		foreach (Track track in m_Tracks)
			track.Sample(startTime, finishTime);
		
		m_Time = finishTime;
	}
}