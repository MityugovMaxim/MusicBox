using System;
using UnityEngine;
using Zenject;

public class SongPreview : MonoBehaviour
{
	public event Action<string> OnPlay;
	public event Action<string> OnStop;

	[Inject] PreviewProcessor m_PreviewProcessor;
	[Inject] AmbientManager m_AmbientManager;

	string m_SongID;

	public void Play(string _SongID)
	{
		m_SongID = _SongID;
		
		m_PreviewProcessor.Play(m_SongID);
		m_AmbientManager.Pause();
		
		OnPlay?.Invoke(m_SongID);
	}

	public void Stop()
	{
		m_PreviewProcessor.Stop();
		m_AmbientManager.Play();
		
		OnStop?.Invoke(m_SongID);
	}
}
