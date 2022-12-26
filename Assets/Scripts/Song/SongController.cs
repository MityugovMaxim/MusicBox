using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;
using Object = UnityEngine.Object;

[Preserve]
public class SongController
{
	[Inject] AudioManager       m_AudioManager;
	[Inject] ConfigProcessor    m_ConfigProcessor;
	[Inject] SongsManager       m_SongsManager;
	[Inject] AudioClipProvider  m_AudioClipProvider;
	[Inject] ASFProvider        m_ASFProvider;
	[Inject] SongPlayer.Factory m_SongFactory;
	[Inject] HealthController   m_HealthController;
	[Inject] ScoreController    m_ScoreController;

	string        m_SongID;
	Action<float> m_Progress;
	SongPlayer    m_Player;

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
		
		if (m_Player != null)
		{
			m_Player.Stop();
			m_Player.Clear();
			Object.Destroy(m_Player.gameObject);
		}
		
		m_Player = null;
		
		await ResourceManager.UnloadAsync();
		
		SongPlayer player = await ResourceManager.LoadAsync<SongPlayer>("default");
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
		
		ASFFile asf = await LoadASFAsync(m_SongID);
		
		if (asf == null)
		{
			Log.Error(this, "Load song failed. ASF with ID '{0}' is null.", m_SongID);
			return false;
		}
		
		m_Progress = null;
		
		float    ratio    = m_ConfigProcessor.SongRatio;
		float    speed    = m_SongsManager.GetSpeed(m_SongID);
		RankType songRank = m_SongsManager.GetRank(m_SongID);
		
		m_Player = m_SongFactory.Create(player);
		m_Player.Setup(ratio, speed, music, asf, Finish);
		
		m_ScoreController.Setup(songRank, asf);
		m_HealthController.Setup(Death);
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Sample();
		
		await UnityTask.Yield();
		
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
		
		m_HealthController.Restore();
		m_ScoreController.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Play(GetLatency());
		
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
		
		m_HealthController.Restore();
		
		await Rewind(token);
		
		if (token.IsCancellationRequested)
			return;
		
		m_Player.Play(GetLatency());
		
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
		
		m_HealthController.Restore();
		m_ScoreController.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Clear();
		m_Player.Play(GetLatency());
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
		
		Object.Destroy(m_Player.gameObject);
		
		m_Player = null;
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
		
		Object.Destroy(m_Player.gameObject);
		
		m_Player = null;
	}

	void Death()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Death failed. Player is null.");
			return;
		}
		
		m_RewindToken?.Cancel();
		m_RewindToken?.Dispose();
		m_RewindToken = null;
		
		m_Player.Stop();
	}

	void Finish()
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

	Task<AudioClip> LoadMusicAsync(string _SongID)
	{
		string path = m_SongsManager.GetMusic(_SongID);
		
		return m_AudioClipProvider.DownloadAsync(path, ProcessMusicProgress);
	}

	Task<ASFFile> LoadASFAsync(string _SongID)
	{
		string path = m_SongsManager.GetASF(_SongID);
		
		return m_ASFProvider.DownloadAsync(path, ProcessASFProgress);
	}

	void ProcessMusicProgress(float _Progress)
	{
		float progress = MathUtility.Remap(_Progress, 0, 1, 0, 0.95f);
		
		m_Progress?.Invoke(progress);
	}

	void ProcessASFProgress(float _Progress)
	{
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
}
