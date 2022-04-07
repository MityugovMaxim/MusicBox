using System;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SongController
{
	[Inject] UISongContainer    m_SongContainer;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] StorageProcessor   m_StorageProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] HealthManager      m_HealthManager;
	[Inject] ScoreManager       m_ScoreManager;
	[Inject] SongPlayer.Factory m_SongFactory;
	[Inject] AmbientProcessor   m_AmbientProcessor;
	[Inject] MusicProcessor     m_MusicProcessor;

	string     m_SongID;
	SongPlayer m_Player;

	CancellationTokenSource m_LoadToken;
	CancellationTokenSource m_RewindToken;

	public async Task<bool> Load(string _SongID)
	{
		m_SongID = _SongID;
		
		if (string.IsNullOrEmpty(m_SongID))
		{
			Log.Error(this, "Load failed. Song ID is null or empty.");
			return false;
		}
		
		string skin = m_SongsProcessor.GetSkin(m_SongID);
		
		if (string.IsNullOrEmpty(skin))
		{
			Log.Error(this, "Load failed. Skin with ID '{0}' is null or empty.", m_SongID);
			return false;
		}
		
		m_LoadToken?.Cancel();
		m_LoadToken?.Dispose();
		
		m_LoadToken = new CancellationTokenSource();
		
		if (m_Player != null)
			GameObject.DestroyImmediate(m_Player.gameObject);
		
		m_Player = null;
		
		CancellationToken token = m_LoadToken.Token;
		
		await ResourceManager.UnloadAsync(token);
		
		m_Player = await ResourceManager.InstantiateAsync(
			skin,
			m_SongFactory,
			m_SongContainer,
			token
		);
		
		AudioClip music = await LoadMusicAsync(m_SongID, token);
		
		string asf = await LoadASFAsync(m_SongID, token);
		
		float ratio    = m_SongsProcessor.GetRatio(m_SongID);
		float speed    = m_SongsProcessor.GetSpeed(m_SongID);
		float duration = m_Player.Size / speed;
		
		m_Player.Setup(ratio, duration, music, asf, Finish);
		
		m_HealthManager.Setup(m_SongID);
		m_ScoreManager.Setup(m_SongID);
		
		UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>();
		if (gameMenu != null)
			gameMenu.Setup(m_SongID);
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>();
		if (pauseMenu != null)
			pauseMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		
		m_LoadToken?.Dispose();
		m_LoadToken = null;
		
		return true;
	}

	public void Start()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Play failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_HealthManager.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Play();
		
		DisableAudio();
	}

	public void Pause()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Pause failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		DisableAudio();
	}

	public async void Resume()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Resume failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		
		m_RewindToken = new CancellationTokenSource();
		
		CancellationToken token = m_RewindToken.Token;
		
		await Rewind(token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_Player.Play();
		
		DisableAudio();
		
		m_RewindToken?.Dispose();
		m_RewindToken = null;
	}

	public async void Revive()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Revive failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		
		m_RewindToken = new CancellationTokenSource();
		
		CancellationToken token = m_RewindToken.Token;
		
		m_HealthManager.Restore();
		
		await Rewind(token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_Player.Play();
		
		DisableAudio();
		
		m_RewindToken?.Dispose();
		m_RewindToken = null;
	}

	public void Restart()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Restart failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_HealthManager.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Clear();
		m_Player.Play();
		
		DisableAudio();
	}

	public void Leave()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Leave failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		GameObject.DestroyImmediate(m_Player.gameObject);
		
		m_Player = null;
		
		EnableAudio();
	}

	async void Finish()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Finish failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		EnableAudio();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		if (resultMenu != null)
			resultMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.ResultMenu);
	}

	Task Rewind(CancellationToken _Token = default)
	{
		if (m_Player == null)
		{
			Log.Error(this, "Rewind failed. Player is null.");
			return null;
		}
		
		const float duration = 1.0f;
		
		float limit  = -m_Player.Duration;
		float offset = m_Player.Duration * m_Player.Ratio;
		
		double source = m_Player.Time;
		double target = Math.Max(limit, m_Player.Time - offset);
		
		try
		{
			return UnityTask.Phase(
				_Phase => m_Player.Time = ASFMath.Lerp(source, target, _Phase),
				duration,
				_Token
			);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		return null;
	}

	Task<AudioClip> LoadMusicAsync(string _SongID, CancellationToken _Token = default)
	{
		string path = $"Songs/{_SongID}.ogg";
		
		return m_StorageProcessor.LoadAudioClipAsync(path, _Token);
	}

	async Task<string> LoadASFAsync(string _SongID, CancellationToken _Token = default)
	{
		string path = $"Songs/{_SongID}.asf";
		
		return await m_StorageProcessor.LoadJson(path, _Token);
	}

	void DisableAudio()
	{
		m_MusicProcessor.StopPreview();
		m_AmbientProcessor.Unlock();
		m_AmbientProcessor.Pause();
		m_AmbientProcessor.Lock();
	}

	void EnableAudio()
	{
		m_MusicProcessor.StopPreview();
		m_AmbientProcessor.Unlock();
		m_AmbientProcessor.Resume();
	}
}