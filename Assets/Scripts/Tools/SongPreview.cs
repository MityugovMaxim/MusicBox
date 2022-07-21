using System;
using UnityEngine;
using Zenject;

public class SongPreview : MonoBehaviour
{
	public event Action<string> OnPlay;
	public event Action<string> OnStop;

	[Inject] PreviewProcessor m_PreviewProcessor;
	[Inject] AmbientProcessor m_AmbientProcessor;

	string m_SongID;

	public void Play(string _SongID)
	{
		m_SongID = _SongID;
		
		m_PreviewProcessor.Play(m_SongID);
		m_AmbientProcessor.Pause();
		
		OnPlay?.Invoke(m_SongID);
	}

	public void Stop()
	{
		m_PreviewProcessor.Stop();
		m_AmbientProcessor.Resume();
		
		OnStop?.Invoke(m_SongID);
	}
}