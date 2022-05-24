using UnityEngine;
using Zenject;

public class SongPreview : MonoBehaviour
{
	[Inject] PreviewProcessor m_PreviewProcessor;
	[Inject] AmbientProcessor m_AmbientProcessor;

	bool m_Ambient;

	public void Play(string _SongID)
	{
		m_PreviewProcessor.Play(_SongID);
		m_AmbientProcessor.Pause();
	}

	public void Stop()
	{
		m_PreviewProcessor.Stop();
		m_AmbientProcessor.Resume();
	}
}