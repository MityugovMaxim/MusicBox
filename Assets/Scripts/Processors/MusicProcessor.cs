using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

public class MusicProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION = 0.5f;
	const float STOP_FADE_DURATION = 0.25f;

	[Inject] StorageProcessor m_StorageProcessor;

	AudioSource m_AudioSource;

	CancellationTokenSource m_TokenSource;

	void Awake()
	{
		m_AudioSource             = gameObject.AddComponent<AudioSource>();
		m_AudioSource.loop        = true;
		m_AudioSource.playOnAwake = false;
		m_AudioSource.volume      = 0;
	}

	public async void PlayPreview(string _SongID)
	{
		await PlayPreviewAsync(_SongID);
	}

	public async void StopPreview()
	{
		await StopPreviewAsync();
	}

	Task PlayPreviewAsync(string _SongID)
	{
		string path = $"Previews/{_SongID}.ogg";
		
		return PlayMusicAsync(path);
	}

	Task StopPreviewAsync()
	{
		return StopMusicAsync();
	}

	async Task PlayMusicAsync(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync(_Path, token);
			
			if (audioClip == null || token.IsCancellationRequested)
				return;
			
			m_AudioSource.Stop();
			m_AudioSource.clip = audioClip;
			m_AudioSource.Play();
			
			await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
			
			await m_AudioSource.SetVolumeAsync(1, PLAY_FADE_DURATION, token);
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

	async Task StopMusicAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		m_AudioSource.Stop();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}