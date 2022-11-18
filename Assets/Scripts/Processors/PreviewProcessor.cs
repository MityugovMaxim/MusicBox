using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class PreviewProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION = 0.5f;
	const float STOP_FADE_DURATION = 0.5f;

	[Inject] SongsManager     m_SongsManager;
	[Inject] StorageProcessor m_StorageProcessor;

	AudioSource m_AudioSource;
	string      m_SongID;

	CancellationTokenSource m_TokenSource;

	void Awake()
	{
		m_AudioSource             = gameObject.AddComponent<AudioSource>();
		m_AudioSource.loop        = true;
		m_AudioSource.playOnAwake = false;
		m_AudioSource.volume      = 0;
	}

	public async void Play(string _SongID)
	{
		if (m_SongID == _SongID)
			return;
		
		m_SongID = _SongID;
		
		await PlayAsync();
	}

	public async void Stop()
	{
		m_SongID = null;
		
		await StopAsync();
	}

	void CancelAudio()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async Task PlayAsync()
	{
		CancelAudio();
		
		if (string.IsNullOrEmpty(m_SongID))
			return;
		
		string path = m_SongsManager.GetPreview(m_SongID);
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync(path, null, CancellationToken.None);
			
			if (token.IsCancellationRequested)
				return;
			
			if (m_AudioSource.isPlaying)
			{
				await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
				
				m_AudioSource.Stop();
			}
			
			if (token.IsCancellationRequested)
				return;
			
			m_AudioSource.clip   = audioClip;
			m_AudioSource.volume = 0;
			
			if (audioClip != null)
			{
				m_AudioSource.Play();
				
				await m_AudioSource.SetVolumeAsync(1, PLAY_FADE_DURATION, token);
			}
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async Task StopAsync()
	{
		CancelAudio();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			if (m_AudioSource.isPlaying && m_AudioSource.volume > float.Epsilon)
				await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
			
			m_AudioSource.Stop();
			
			m_AudioSource.clip   = null;
			m_AudioSource.volume = 0;
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}
