using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.LoadingMenu)]
public class UILoadingMenu : UIMenu
{
	[Inject] SongController m_SongController;
	[Inject] MenuProcessor  m_MenuProcessor;

	string m_SongID;

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
	}

	protected override async void OnShowFinished()
	{
		await m_MenuProcessor.Hide(MenuType.MainMenu, true);
		await m_MenuProcessor.Hide(MenuType.SongMenu, true);
		
		bool success = false;
		try
		{
			success = await m_SongController.Load(m_SongID);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
		}
		
		if (success)
		{
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
		else
		{
			UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
			
			songMenu.Setup(m_SongID);
			
			await m_MenuProcessor.Show(MenuType.MainMenu, true);
			await m_MenuProcessor.Show(MenuType.SongMenu, true);
			
			await m_MenuProcessor.ErrorLocalizedAsync("song_load_error", "SONG_LOAD_ERROR_TITLE", "SONG_LOAD_ERROR_MESSAGE");
			
			await m_MenuProcessor.Hide(MenuType.LoadingMenu);
		}
	}

	protected override void OnHideFinished()
	{
		m_SongController.Start();
	}
}