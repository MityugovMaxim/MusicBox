using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongDownload : UIGroup
{
	public string SongID
	{
		get => m_SongID;
		set
		{
			if (m_SongID == value)
				return;
			
			m_SongID = value;
			
			ProcessState();
		}
	}

	[SerializeField] UIGroup    m_DownloadGroup;
	[SerializeField] UIGroup    m_ProgressGroup;
	[SerializeField] UIGroup    m_CompleteGroup;
	[SerializeField] Button     m_DownloadButton;
	[SerializeField] UIProgress m_Progress;

	[Inject] AudioClipProvider m_AudioClipProvider;
	[Inject] ASFProvider       m_ASFProvider;
	[Inject] SongsManager      m_SongsManager;
	[Inject] MenuProcessor     m_MenuProcessor;

	string m_SongID;

	CancellationTokenSource m_TokenSource;

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
		
		SongID = null;
		
		m_TokenSource?.Cancel();
	}

	async void Download()
	{
		m_TokenSource?.Cancel();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		string songID = SongID;
		string music  = m_SongsManager.GetMusic(SongID);
		string asf    = m_SongsManager.GetASF(SongID);
		
		try
		{
			m_ProgressGroup.Show();
			m_DownloadGroup.Hide();
			m_CompleteGroup.Hide();
			
			await m_AudioClipProvider.DownloadAsync(music, ProcessMusicProgress, token);
			
			await m_ASFProvider.DownloadAsync(asf, ProcessASFProgress, token);
			
			token.ThrowIfCancellationRequested();
			
			if (songID != SongID)
				return;
			
			m_DownloadGroup.Hide();
			m_ProgressGroup.Hide();
			
			await m_CompleteGroup.ShowAsync();
			
			await Task.Delay(1000, token);
			
			Hide();
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			m_DownloadGroup.Show();
			m_ProgressGroup.Hide();
			m_CompleteGroup.Hide();
			
			await m_MenuProcessor.ErrorAsync("song_download");
		}
	}

	void ProcessState()
	{
		m_TokenSource?.Cancel();
		
		m_Progress.Progress = 0;
		
		string music = m_SongsManager.GetMusic(SongID);
		string asf   = m_SongsManager.GetASF(SongID);
		
		if (m_AudioClipProvider.IsDownloaded(music) && m_ASFProvider.IsDownloaded(asf))
		{
			Hide(true);
			
			m_CompleteGroup.Hide(true);
			m_ProgressGroup.Hide(true);
			m_DownloadGroup.Hide(true);
		}
		else if (m_AudioClipProvider.IsDownloading(music) || m_ASFProvider.IsDownloading(asf))
		{
			Download();
			
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
