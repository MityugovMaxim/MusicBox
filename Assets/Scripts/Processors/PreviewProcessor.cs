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

	public async void Play(string _SongID)
	{
		await PlayAsync(_SongID);
	}

	public async void Stop()
	{
		await StopAsync();
	}

	async Task PlayAsync(string _SongID)
	{
		string path = !string.IsNullOrEmpty(_SongID) ? $"Previews/{_SongID}.ogg" : string.Empty;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync(path, null, token);
			
			if (token.IsCancellationRequested)
				return;
			
			await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
			
			m_AudioSource.Stop();
			m_AudioSource.clip = audioClip;
			
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
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
			
			m_AudioSource.Stop();
			m_AudioSource.clip = null;
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