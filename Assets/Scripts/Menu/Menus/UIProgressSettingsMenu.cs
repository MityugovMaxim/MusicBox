using System;
using AudioBox.Logging;
using UnityEngine;
using Zenject;

[Menu(MenuType.ProgressSettingsMenu)]
public class UIProgressSettingsMenu : UIMenu
{
	[SerializeField] UISerializedObject m_Fields;

	[Inject] ProgressProcessor m_ProgressProcessor;
	[Inject] MenuProcessor     m_MenuProcessor;

	int              m_Level;
	ProgressSnapshot m_Snapshot;

	public void Setup(int _Level)
	{
		m_Level = _Level;
		
		m_Fields.Clear();
		
		m_Snapshot = m_ProgressProcessor.GetSnapshot(m_Level);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"LEVEL: {m_Level}", m_Snapshot);
	}

	public async void Back()
	{
		await m_MenuProcessor.Show(MenuType.ProgressMenu, true);
		await m_MenuProcessor.Hide(MenuType.ProgressSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_ProgressProcessor.Load();
		
		Setup(m_Level);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_ProgressProcessor.Upload(m_Snapshot.Level);
			
			Setup(m_Snapshot.Level);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload failed");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_progress",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}