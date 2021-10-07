using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class LevelPreviewAudioSource : MonoBehaviour
{
	StorageProcessor m_StorageProcessor;
	MusicProcessor   m_MusicProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(
		StorageProcessor _StorageProcessor,
		MusicProcessor   _MusicProcessor
	)
	{
		m_StorageProcessor = _StorageProcessor;
		m_MusicProcessor   = _MusicProcessor;
	}

	CancellationTokenSource m_TokenSource;

	public async void Play(string _LevelID)
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_LevelID = _LevelID;
		
		try
		{
			Task pauseAmbientTask = m_MusicProcessor.PauseAmbient(token);
			Task stopMusicTask    = m_MusicProcessor.StopMusic(token);
			
			await Task.WhenAll(pauseAmbientTask, stopMusicTask);
			
			AudioClip preview = await m_StorageProcessor.LoadLevelPreview(m_LevelID, token);
			
			await m_MusicProcessor.PlayMusic(preview, token);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[MusicProcessor] Play canceled. Level ID: {0}", _LevelID);
		}
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}

	public async void Stop()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			Task stopMusicTask   = m_MusicProcessor.StopMusic(token);
			Task playAmbientTask = m_MusicProcessor.PlayAmbient(token);
			
			await Task.WhenAll(stopMusicTask, playAmbientTask);
		}
		catch (TaskCanceledException)
		{
			Debug.Log("[MusicProcessor] Stop canceled.");
		}
		finally
		{
			m_TokenSource?.Dispose();
			m_TokenSource = null;
		}
	}
}