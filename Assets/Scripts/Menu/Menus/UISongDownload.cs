using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongDownload : UIGroup
{
	string MusicPath => !string.IsNullOrEmpty(m_SongID) ? $"Songs/{m_SongID}.ogg" : string.Empty;
	string ASFPath   => !string.IsNullOrEmpty(m_SongID) ? $"Songs/{m_SongID}.asf" : string.Empty;

	[SerializeField] UIGroup    m_DownloadGroup;
	[SerializeField] UIGroup    m_ProgressGroup;
	[SerializeField] UIGroup    m_CompleteGroup;
	[SerializeField] Button     m_DownloadButton;
	[SerializeField] UIProgress m_Progress;

	[Inject] StorageProcessor m_StorageProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string m_SongID;

	protected override void Awake()
	{
		base.Awake();
		
		m_DownloadButton.onClick.AddListener(Download);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_DownloadButton.onClick.RemoveListener(Download);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_StorageProcessor.Unsubscribe(MusicPath, ProcessMusicProgress);
		m_StorageProcessor.Unsubscribe(ASFPath, ProcessASFProgress);
		
		m_SongID = null;
	}

	public void Setup(string _SongID)
	{
		m_StorageProcessor.Unsubscribe(MusicPath, ProcessMusicProgress);
		m_StorageProcessor.Unsubscribe(ASFPath, ProcessASFProgress);
		
		m_SongID = _SongID;
		
		RestoreProgress();
		
		if (m_StorageProcessor.IsLoaded(MusicPath) && m_StorageProcessor.IsLoaded(ASFPath))
		{
			Hide(true);
			
			m_CompleteGroup.Hide(true);
			m_ProgressGroup.Hide(true);
			m_DownloadGroup.Hide(true);
		}
		else if (m_StorageProcessor.IsLoading(MusicPath) || m_StorageProcessor.IsLoading(ASFPath))
		{
			m_StorageProcessor.Subscribe(MusicPath, ProcessMusicProgress);
			m_StorageProcessor.Subscribe(ASFPath, ProcessASFProgress);
			
			Show(true);
			
			m_ProgressGroup.Show(true);
			
			m_CompleteGroup.Hide(true);
			m_DownloadGroup.Hide(true);
		}
		else
		{
			Show(true);
			
			m_DownloadGroup.Show(true);
			
			m_CompleteGroup.Hide(true);
			m_ProgressGroup.Hide(true);
		}
	}

	async void Download()
	{
		RestoreProgress();
		
		string musicPath = MusicPath;
		string asfPath   = ASFPath;
		
		try
		{
			m_ProgressGroup.Show();
			
			m_DownloadGroup.Hide();
			m_CompleteGroup.Hide();
			
			await m_StorageProcessor.LoadAudioClipAsync(musicPath, ProcessMusicProgress);
			
			await m_StorageProcessor.LoadJson(asfPath, true, ProcessASFProgress);
			
			if (musicPath != MusicPath || asfPath != ASFPath)
				return;
			
			m_DownloadGroup.Hide();
			m_ProgressGroup.Hide();
			
			await m_CompleteGroup.ShowAsync();
			
			await Task.Delay(1000);
			
			Hide();
		}
		catch (TaskCanceledException)
		{
			m_DownloadGroup.Show();
			m_ProgressGroup.Hide();
			m_CompleteGroup.Hide();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			m_DownloadGroup.Show();
			m_ProgressGroup.Hide();
			m_CompleteGroup.Hide();
			
			await m_MenuProcessor.ErrorLocalizedAsync(
				"song_download",
				"SONG_LOAD_ERROR_TITLE",
				"SONG_LOAD_ERROR_MESSAGE"
			);
		}
	}

	void RestoreProgress()
	{
		m_Progress.Progress = 0;
	}

	void ProcessMusicProgress(float _Progress)
	{
		float progress = MathUtility.Remap(_Progress, 0, 1, 0, 0.95f);
		
		m_Progress.Progress = progress;
	}

	void ProcessASFProgress(float _Progress)
	{
		float progress = MathUtility.Remap(_Progress, 0, 1, 0.95f, 1);
		
		m_Progress.Progress = progress;
	}
}