using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.SongSettingsMenu)]
public class UISongSettingsMenu : UIMenu
{
	[SerializeField] ScrollRect         m_Scroll;
	[SerializeField] UIImageField       m_Image;
	[SerializeField] UIAudioField       m_Preview;
	[SerializeField] UIAudioField       m_Music;
	[SerializeField] UISerializedObject m_Fields;

	[Inject] SongsProcessor m_SongsProcessor;
	[Inject] MenuProcessor  m_MenuProcessor;

	string       m_SongID;
	SongSnapshot m_Snapshot;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Image.Label = "Album artwork";
		m_Image.Setup($"Thumbnails/Songs/{m_SongID}.jpg");
		
		m_Preview.Label = "Preview";
		m_Preview.Setup($"Previews/{m_SongID}.ogg");
		
		m_Music.Label = "Music";
		m_Music.Setup($"Songs/{m_SongID}.ogg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_SongsProcessor.GetSnapshot(m_SongID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"Song: {m_SongID}", m_Snapshot);
	}

	public async void Back()
	{
		await m_MenuProcessor.Hide(MenuType.SongSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_SongsProcessor.Load();
		
		Setup(m_SongID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_SongsProcessor.Upload(m_SongID, m_Snapshot.ID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload song failed. Song ID: '{0}'.", m_SongID);
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"song_upload",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}