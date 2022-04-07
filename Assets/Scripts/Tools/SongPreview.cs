using UnityEngine;
using Zenject;

public class SongPreview : MonoBehaviour
{
	[Inject] MusicProcessor   m_MusicProcessor;
	[Inject] AmbientProcessor m_AmbientProcessor;

	bool m_Ambient;

	public void Play(string _SongID)
	{
		m_MusicProcessor.PlayPreview(_SongID);
		m_AmbientProcessor.Pause();
	}

	public void Stop()
	{
		m_MusicProcessor.StopPreview();
		m_AmbientProcessor.Resume();
	}
}