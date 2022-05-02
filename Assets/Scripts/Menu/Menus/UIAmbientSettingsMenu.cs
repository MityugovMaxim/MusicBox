using System;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.AmbientSettingsMenu)]
public class UIAmbientSettingsMenu : UIMenu
{
	[SerializeField] UIAudioField       m_Ambient;
	[SerializeField] UISerializedObject m_Fields;
	[SerializeField] ScrollRect         m_Scroll;

	[Inject] AmbientProcessor m_AmbientProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string          m_AmbientID;
	AmbientSnapshot m_Snapshot;

	public void Setup(string _AmbientID)
	{
		m_AmbientID = _AmbientID;
		
		m_Scroll.verticalNormalizedPosition = 1;
		
		m_Ambient.Label = "Ambient";
		m_Ambient.Setup($"Ambient/{m_AmbientID}.ogg");
		
		m_Fields.Clear();
		
		m_Snapshot = m_AmbientProcessor.GetSnapshot(m_AmbientID);
		
		if (m_Snapshot == null)
			return;
		
		m_Fields.Add($"Ambient: {m_AmbientID}", m_Snapshot);
	}

	public async void Back()
	{
		await m_MenuProcessor.Show(MenuType.AmbientMenu, true);
		await m_MenuProcessor.Hide(MenuType.AmbientSettingsMenu);
	}

	public async void Restore()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await m_AmbientProcessor.Load();
		
		Setup(m_AmbientID);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_AmbientProcessor.Upload(m_AmbientID, m_Snapshot.ID);
			
			Setup(m_Snapshot.ID);
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload ambient failed.");
			
			string message = exception.GetBaseException().Message;
			
			await m_MenuProcessor.ErrorAsync(
				"upload_ambient",
				"Upload failed",
				message
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	protected override void OnShowFinished()
	{
		m_AmbientProcessor.Pause();
		m_AmbientProcessor.Lock();
	}

	protected override void OnHideStarted()
	{
		m_AmbientProcessor.Unlock();
		m_AmbientProcessor.Resume();
	}
}