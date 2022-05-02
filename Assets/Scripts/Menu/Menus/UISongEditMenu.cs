using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Logging;
using Facebook.MiniJSON;
using UnityEngine;
using Zenject;

[Menu(MenuType.SongEditMenu)]
public class UISongEditMenu : UIMenu
{
	[SerializeField] UIPlayer    m_Player;
	[SerializeField] UIAudioWave m_Background;
	[SerializeField] UIBeat      m_Beat;

	[Inject] StorageProcessor m_StorageProcessor;
	[Inject] SongsProcessor   m_SongsProcessor;
	[Inject] ConfigProcessor  m_ConfigProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;
	[Inject] AudioManager     m_AudioManager;
	[Inject] AmbientProcessor m_AmbientProcessor;

	string m_SongID;
	double m_Time;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	public async Task Load()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		AudioClip music;
		try
		{
			music = await LoadMusic();
		}
		catch (Exception)
		{
			music = null;
		}
		
		string asf;
		try
		{
			asf = await LoadASF();
		}
		catch (Exception)
		{
			asf = string.Empty;
		}
		
		float ratio    = m_ConfigProcessor.SongRatio;
		float bpm      = m_SongsProcessor.GetBPM(m_SongID);
		float speed    = m_SongsProcessor.GetSpeed(m_SongID);
		float duration = RectTransform.rect.height / speed;
		
		m_Player.Setup(ratio, duration, music, asf);
		m_Player.Time = 0;
		m_Player.Sample();
		
		m_Background.Ratio     = ratio;
		m_Background.Duration  = duration;
		m_Background.AudioClip = music;
		m_Background.Time      = 0;
		
		m_Beat.Duration = duration;
		m_Beat.Ratio    = ratio;
		m_Beat.BPM      = bpm;
		m_Beat.Time     = 0;
		
		await m_Background.Render();
		
		await m_MenuProcessor.Show(MenuType.SongEditMenu);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Back()
	{
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		songMenu.Setup(m_SongID);
		
		await m_MenuProcessor.Show(MenuType.SongMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.SongEditMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		Dictionary<string, object> data = m_Player.Serialize();
		
		string path = $"Songs/{m_SongID}.asf";
		
		string asf = Json.Serialize(data);
		
		try
		{
			await m_StorageProcessor.UploadJson(path, asf, Encoding.UTF8);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload ASF failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_asf",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		string asf = await LoadASF();
		
		m_Player.Clear();
		m_Player.Deserialize(asf);
		m_Player.Sample();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public void Play()
	{
		float latency = m_AudioManager.GetLatency();
		
		m_Time = m_Player.Time;
		
		m_Player.Play(latency);
	}

	public void Stop()
	{
		m_Player.Time = m_Time;
		
		m_Player.Stop();
	}

	protected override async void OnShowFinished()
	{
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		m_AmbientProcessor.Pause();
	}

	Task<AudioClip> LoadMusic()
	{
		string path = $"Songs/{m_SongID}.ogg";
		
		return m_StorageProcessor.LoadAudioClipAsync(path);
	}

	Task<string> LoadASF()
	{
		string path = $"Songs/{m_SongID}.asf";
		
		return m_StorageProcessor.LoadJson(path);
	}
}