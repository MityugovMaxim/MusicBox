using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class MusicProcessor : MonoBehaviour
{
	const float PLAY_FADE_DURATION = 0.5f;
	const float STOP_FADE_DURATION = 0.25f;

	AudioSource m_AudioSource;

	StorageProcessor m_StorageProcessor;

	CancellationTokenSource m_TokenSource;

	[Inject]
	public void Construct(StorageProcessor _StorageProcessor)
	{
		m_StorageProcessor = _StorageProcessor;
		
		m_AudioSource             = gameObject.AddComponent<AudioSource>();
		m_AudioSource.loop        = true;
		m_AudioSource.playOnAwake = false;
		m_AudioSource.volume      = 0;
	}

	public async void PlayPreview(string _LevelID)
	{
		await PlayPreviewAsync(_LevelID);
	}

	public async void StopPreview()
	{
		await StopPreviewAsync();
	}

	public Task PlayPreviewAsync(string _LevelID)
	{
		string path = $"Previews/{_LevelID}.ogg";
		
		return PlayMusicAsync(path);
	}

	public Task StopPreviewAsync()
	{
		return StopMusicAsync();
	}

	public async Task PlayMusicAsync(string _Path)
	{
		if (string.IsNullOrEmpty(_Path))
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		AudioClip audioClip = await m_StorageProcessor.LoadAudioClipAsync(_Path, token);
		
		if (token.IsCancellationRequested)
			return;
		
		if (audioClip == null)
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
			return;
		}
		
		m_AudioSource.Stop();
		m_AudioSource.clip = audioClip;
		m_AudioSource.Play();
		
		await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
		
		await m_AudioSource.SetVolumeAsync(1, PLAY_FADE_DURATION, token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public async Task StopMusicAsync()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		await m_AudioSource.SetVolumeAsync(0, STOP_FADE_DURATION, token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_AudioSource.Stop();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}