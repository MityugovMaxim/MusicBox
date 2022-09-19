using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongCreateMusicPage : UISongCreateMenuPage
{
	public override bool Valid => m_Music != null && m_Music.length >= 30;

	[SerializeField] AudioSource m_AudioSource;
	[SerializeField] UIGroup     m_ControlGroup;
	[SerializeField] UIAudioWave m_Wave;
	[SerializeField] Button      m_SelectButton;
	[SerializeField] Button      m_PlayButton;
	[SerializeField] Button      m_StopButton;

	[Inject] IFileManager  m_FileManager;
	[Inject] MenuProcessor m_MenuProcessor;

	AudioClip m_Music;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_SelectButton.Subscribe(Select);
		m_PlayButton.Subscribe(Play);
		m_StopButton.Subscribe(Stop);
	}

	public byte[] CreateMusic()
	{
		return m_Music != null ? m_Music.EncodeToOGG(0.85f) : null;
	}

	async void Select()
	{
		Cancel();
		
		string path = null;
		
		try
		{
			path = await m_FileManager.SelectFile(FileManagerUtility.AudioExtensions);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		if (string.IsNullOrEmpty(path))
			return;
		
		await m_ControlGroup.HideAsync();
		
		m_Music = await WebRequest.LoadAudioClipFile(path, AudioType.UNKNOWN);
		
		if (m_Music == null)
			return;
		
		m_Music.LoadAudioData();
		
		await m_Wave.RenderAsync(m_Music, m_Music.length);
		
		m_AudioSource.clip = m_Music;
		
		m_ControlGroup.Show();
	}

	async void Play()
	{
		Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		m_PlayButton.gameObject.SetActive(false);
		
		m_StopButton.gameObject.SetActive(true);
		
		try
		{
			await m_AudioSource.PlayAsync(m_TokenSource.Token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		catch (OperationCanceledException)
		{
			return;
		}
		finally
		{
			m_PlayButton.gameObject.SetActive(true);
			
			m_StopButton.gameObject.SetActive(false);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void Stop()
	{
		Cancel();
		
		m_AudioSource.Stop();
		
		m_StopButton.gameObject.SetActive(false);
		
		m_PlayButton.gameObject.SetActive(true);
	}

	void Cancel()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}
}