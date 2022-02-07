using UnityEngine;
using Zenject;

public class LevelPreview : MonoBehaviour
{
	AmbientProcessor m_AmbientProcessor;
	MusicProcessor   m_MusicProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		AmbientProcessor _AmbientProcessor,
		MusicProcessor   _MusicProcessor
	)
	{
		m_AmbientProcessor = _AmbientProcessor;
		m_MusicProcessor   = _MusicProcessor;
	}

	public void Play(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_AmbientProcessor.Pause();
		m_MusicProcessor.PlayPreview(m_LevelID);
	}

	public void Stop()
	{
		m_AmbientProcessor.Resume();
		m_MusicProcessor.StopPreview();
	}
}