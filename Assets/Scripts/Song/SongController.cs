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
	[Inject] AudioManager       m_AudioManager;
	[Inject] ConfigProcessor    m_ConfigProcessor;
	[Inject] SongsProcessor     m_SongsProcessor;
	[Inject] StorageProcessor   m_StorageProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] SongPlayer.Factory m_SongFactory;
	[Inject] SoundProcessor     m_SoundProcessor;
	[Inject] AmbientProcessor   m_AmbientProcessor;
	[Inject] PreviewProcessor   m_PreviewProcessor;
	[Inject] ProgressProcessor  m_ProgressProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	[Inject] HealthManager m_HealthManager;
	[Inject] ScoreManager  m_ScoreManager;

	string        m_SongID;
	Action<float> m_Progress;
	float         m_Loading;
	float         m_PingTime;
	SongPlayer    m_Player;

	float m_LoadTime;
	int   m_AdsReviveCount;

	CancellationTokenSource m_RewindToken;

	public async Task<bool> Load(string _SongID, Action<float> _Progress)
	{
		m_SongID   = _SongID;
		m_Progress = _Progress;
		
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
		
		if (m_Player != null)
		{
			m_Player.Stop();
			m_Player.Clear();
			GameObject.Destroy(m_Player.gameObject);
		}
		
		m_Player = null;
		
		await ResourceManager.UnloadAsync();
		
		SongPlayer player = await ResourceManager.LoadAsync<SongPlayer>(skin);
		if (ReferenceEquals(player, null))
		{
			Log.Error(this, "Load song failed. Player with ID '{0}' is null.", m_SongID);
			return false;
		}
		
		AudioClip music = await LoadMusicAsync(m_SongID);
		
		if (music == null)
		{
			Log.Error(this, "Load song failed. Music with ID '{0}' is null.", m_SongID);
			return false;
		}
		
		string asf = await LoadASFAsync(m_SongID);
		
		if (string.IsNullOrEmpty(asf))
		{
			Log.Error(this, "Load song failed. ASF with ID '{0}' is null.", m_SongID);
			return false;
		}
		
		m_Progress = null;
		
		float ratio = m_ConfigProcessor.SongRatio;
		float speed = m_SongsProcessor.GetSpeed(m_SongID);
		
		m_Player = m_SongFactory.Create(player);
		m_Player.Setup(ratio, speed, music, asf, Finish);
		
		m_ScoreManager.Setup(m_SongID);
		m_HealthManager.Setup(Death);
		
		UIGameMenu gameMenu = m_MenuProcessor.GetMenu<UIGameMenu>();
		if (gameMenu != null)
			gameMenu.Setup(m_SongID);
		
		UIPauseMenu pauseMenu = m_MenuProcessor.GetMenu<UIPauseMenu>();
		if (pauseMenu != null)
			pauseMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.GameMenu, true);
		
		m_Player.AddSampler(gameMenu.Sampler);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Sample();
		
		await UnityTask.Yield();
		
		m_LoadTime       = Time.time;
		m_AdsReviveCount = 0;
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongStart);
		
		m_StatisticProcessor.LogSongStart(
			m_SongID,
			m_SongsProcessor.GetNumber(m_SongID),
			m_ProgressProcessor.GetSongLevel(m_SongID),
			m_SongsProcessor.GetPrice(m_SongID) > 0 ? "paid" : "free"
		);
		
		return true;
	}

	public bool Start()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Play failed. Player is null.");
			return false;
		}
		
		if (m_Player.State == ASFPlayerState.Play)
			return false;
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_HealthManager.Restore();
		m_ScoreManager.Restore();
		
		UIReviveMenu reviveMenu = m_MenuProcessor.GetMenu<UIReviveMenu>();
		if (reviveMenu != null)
			reviveMenu.Setup(m_SongID);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Play(GetLatency());
		
		DisableAudio();
		
		return true;
	}

	public bool Pause()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Pause failed. Player is null.");
			return false;
		}
		
		if (m_Player.State != ASFPlayerState.Play)
			return false;
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		DisableAudio();
		
		return true;
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
		
		m_Player.Play(GetLatency());
		
		DisableAudio();
		
		m_RewindToken?.Dispose();
		m_RewindToken = null;
	}

	public async void Revive(bool _Ads)
	{
		if (m_Player == null)
		{
			Log.Error(this, "Revive failed. Player is null.");
			return;
		}
		
		if (_Ads)
			m_AdsReviveCount++;
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		
		m_RewindToken = new CancellationTokenSource();
		
		CancellationToken token = m_RewindToken.Token;
		
		m_HealthManager.Restore();
		
		await Rewind(token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_Player.Play(GetLatency());
		
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
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongRestart);
		
		m_StatisticProcessor.LogSongFinish(
			m_SongID,
			m_SongsProcessor.GetNumber(m_SongID),
			m_ProgressProcessor.GetSongLevel(m_SongID),
			m_SongsProcessor.GetPrice(m_SongID),
			(int)(m_Player.Time / m_Player.Length * 100),
			m_AdsReviveCount,
			(int)(Time.time - m_LoadTime),
			"restart"
		);
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_HealthManager.Restore();
		m_ScoreManager.Restore();
		
		UIReviveMenu reviveMenu = m_MenuProcessor.GetMenu<UIReviveMenu>();
		if (reviveMenu != null)
			reviveMenu.Setup(m_SongID);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Clear();
		m_Player.Play(GetLatency());
		
		DisableAudio();
	}

	public void Complete()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Complete failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		GameObject.Destroy(m_Player.gameObject);
		
		m_Player = null;
		
		EnableAudio();
	}

	public void Leave()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Leave failed. Player is null.");
			return;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongLeave);
		
		m_StatisticProcessor.LogSongFinish(
			m_SongID,
			m_SongsProcessor.GetNumber(m_SongID),
			m_ProgressProcessor.GetSongLevel(m_SongID),
			m_SongsProcessor.GetPrice(m_SongID),
			(int)(m_Player.Time / m_Player.Length * 100),
			m_AdsReviveCount,
			(int)(Time.time - m_LoadTime),
			"leave"
		);
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		GameObject.Destroy(m_Player.gameObject);
		
		m_Player = null;
		
		EnableAudio();
	}

	async void Death()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Death failed. Player is null.");
			return;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongLose);
		
		m_StatisticProcessor.LogSongFinish(
			m_SongID,
			m_SongsProcessor.GetNumber(m_SongID),
			m_ProgressProcessor.GetSongLevel(m_SongID),
			m_SongsProcessor.GetPrice(m_SongID),
			(int)(m_Player.Time / m_Player.Length * 100),
			m_AdsReviveCount,
			(int)(Time.time - m_LoadTime),
			"lose"
		);
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
		
		await m_MenuProcessor.Show(MenuType.BlockMenu, true);
		
		await Task.Delay(400);
		
		await m_MenuProcessor.Show(MenuType.ReviveMenu);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu, true);
	}

	async void Finish()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Finish failed. Player is null.");
			return;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.SongFinish);
		
		m_StatisticProcessor.LogSongFinish(
			m_SongID,
			m_SongsProcessor.GetNumber(m_SongID),
			m_ProgressProcessor.GetSongLevel(m_SongID),
			m_SongsProcessor.GetPrice(m_SongID),
			(int)(m_Player.Time / m_Player.Length * 100),
			m_AdsReviveCount,
			(int)(Time.time - m_LoadTime),
			"win"
		);
		
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
		
		const float duration = 0.6f;
		
		float limit  = -m_Player.Duration;
		float offset = m_Player.Duration * m_Player.Ratio;
		
		double source = m_Player.Time;
		double target = Math.Max(limit, m_Player.Time - offset);
		
		try
		{
			m_SoundProcessor.Play("Rewind");
			
			return UnityTask.Phase(
				_Phase => m_Player.Time = EaseFunction.EaseOutQuad.Get(source, target, _Phase),
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

	async Task<AudioClip> LoadMusicAsync(string _SongID)
	{
		const float timeout = 15;
		
		string path = m_SongsProcessor.GetMusic(_SongID);
		
		m_Loading  = 0;
		m_PingTime = Time.realtimeSinceStartup;
		
		Task<AudioClip> task = m_StorageProcessor.LoadAudioClipAsync(path, ProcessMusicProgress);
		
		await Task.WhenAny(
			task,
			UnityTask.While(() => Time.realtimeSinceStartup - m_PingTime < timeout)
		);
		
		return task.IsCompletedSuccessfully ? task.Result : null;
	}

	async Task<string> LoadASFAsync(string _SongID)
	{
		const float timeout = 15;
		
		string path = $"Songs/{_SongID}.asf";
		
		m_Loading  = 0;
		m_PingTime = Time.realtimeSinceStartup;
		
		Task<string> task = m_StorageProcessor.LoadJson(path, true, ProcessASFProgress);
		
		await Task.WhenAny(
			task,
			UnityTask.While(() => Time.realtimeSinceStartup - m_PingTime < timeout)
		);
		
		return task.IsCompletedSuccessfully ? task.Result : null;
	}

	void ProcessMusicProgress(float _Progress)
	{
		if (m_Loading < _Progress)
		{
			m_Loading  = _Progress;
			m_PingTime = Time.realtimeSinceStartup;
		}
		
		float progress = MathUtility.Remap(_Progress, 0, 1, 0, 0.95f);
		
		m_Progress?.Invoke(progress);
	}

	void ProcessASFProgress(float _Progress)
	{
		if (m_Loading < _Progress)
		{
			m_Loading  = _Progress;
			m_PingTime = Time.realtimeSinceStartup;
		}
		
		float progress = MathUtility.Remap(_Progress, 0, 1, 0.95f, 1);
		
		m_Progress?.Invoke(progress);
	}

	float GetLatency()
	{
		if (m_AudioManager.HasSettings())
			return m_AudioManager.GetLatency();
		
		if (m_AudioManager.GetAudioOutputType() == AudioOutputType.Bluetooth)
			return m_ConfigProcessor.BluetoothLatency;
		
		return 0;
	}

	void DisableAudio()
	{
		m_PreviewProcessor.Stop();
		m_AmbientProcessor.Unlock();
		m_AmbientProcessor.Pause();
		m_AmbientProcessor.Lock();
	}

	void EnableAudio()
	{
		m_PreviewProcessor.Stop();
		m_AmbientProcessor.Unlock();
		m_AmbientProcessor.Resume();
	}
}
