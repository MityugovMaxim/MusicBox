using System;
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
	[Inject] MenuProcessor      m_MenuProcessor;

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
		
		await ResourceManager.UnloadAsync();
		
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
		
		m_Player = await LoadPlayerAsync(m_SongID, music, asf);
		
		if (m_Player == null)
		{
			Log.Error(this, "Load song failed. Player with ID '{0}' is null.", m_SongID);
			return false;
		}
		
		RankType songRank = m_SongsManager.GetRank(m_SongID);
		
		m_ScoreController.Setup(songRank, asf);
		m_HealthController.Setup(Death);
		
		await UnityTask.Yield();
		
		return true;
	}

	public void Start()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Play failed. Player is null.");
			return;
		}
		
		if (m_Player.State == ASFPlayerState.Play)
			return;
		
		CancelRewind();
		
		m_HealthController.Restore();
		m_ScoreController.Restore();
		
		m_Player.Time = -m_Player.Duration;
		m_Player.Play(GetLatency());
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
		
		CancelRewind();
		
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
		
		await RewindAsync();
		
		m_Player.Play(GetLatency());
	}

	public async void Revive()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Revive failed. Player is null.");
			return;
		}
		
		m_HealthController.Restore();
		
		await RewindAsync();
		
		m_Player.Play(GetLatency());
	}

	public void Restart()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Restart failed. Player is null.");
			return;
		}
		
		CancelRewind();
		
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
		
		CancelRewind();
		
		m_Player.Stop();
		
		Object.Destroy(m_Player.gameObject);
		
		m_Player = null;
	}

	async void Death()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Death failed. Player is null.");
			return;
		}
		
		CancelRewind();
		
		m_Player.Stop();
		
		await m_MenuProcessor.Show(MenuType.ReviveMenu);
	}

	public async void Finish()
	{
		if (m_Player == null)
		{
			Log.Error(this, "Finish failed. Player is null.");
			return;
		}
		
		CancelRewind();
		
		m_Player.Stop();
		
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		
		resultMenu.Setup(m_SongID);
		resultMenu.Show();
		
		await m_MenuProcessor.Show(MenuType.TransitionMenu);
	}

	void CancelRewind()
	{
		m_RewindToken?.Cancel();
	}

	Task RewindAsync()
	{
		CancelRewind();
		
		if (m_Player == null)
		{
			Log.Error(this, "Rewind failed. Player is null.");
			return null;
		}
		
		m_RewindToken = new CancellationTokenSource();
		
		CancellationToken token = m_RewindToken.Token;
		
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
				token
			);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		finally
		{
			m_RewindToken?.Dispose();
			m_RewindToken = null;
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

	async Task<SongPlayer> LoadPlayerAsync(string _SongID, AudioClip _Music, ASFFile _ASF)
	{
		SongPlayer player = await ResourceManager.LoadAsync<SongPlayer>("default");
		
		if (player == null)
			return null;
		
		float speed = m_SongsManager.GetSpeed(_SongID);
		
		player = m_SongFactory.Create(player);
		player.Setup(speed, _Music, _ASF, Finish);
		player.Sample();
		
		return player;
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
