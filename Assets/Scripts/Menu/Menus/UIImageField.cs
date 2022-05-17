using System;
using System.IO;
using System.Threading.Tasks;
using AudioBox.Logging;
using TMPro;
using UnityEngine;
using Zenject;

public class UIImageField : UIEntity
{
	public string Label
	{
		get => m_Label.text;
		set => m_Label.text = value;
	}

	[SerializeField] TMP_Text m_Label;
	[SerializeField] TMP_Text m_File;
	[SerializeField] WebImage m_Image;

	[Inject] IFileManager     m_FileManager;
	[Inject] StorageProcessor m_StorageProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string m_RemotePath;
	string m_LocalPath;

	public void Setup(string _Path)
	{
		m_RemotePath = _Path;
		
		m_Image.Path = m_RemotePath;
		
		m_File.text = !string.IsNullOrEmpty(m_RemotePath)
			? Path.GetFileNameWithoutExtension(m_RemotePath)
			: "Unnamed";
	}

	public async void Upload()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await m_StorageProcessor.UploadFile(m_RemotePath, m_LocalPath);
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Upload image failed. Remote path: {0} Local path: {1}.", m_RemotePath, m_LocalPath);
			
			await m_MenuProcessor.ExceptionAsync("Upload image failed", exception);
		}
		
		m_Image.URL  = false;
		m_Image.Path = m_RemotePath;
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Select()
	{
		m_LocalPath = null;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		try
		{
			m_LocalPath = await m_FileManager.SelectFile("jpg");
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			await m_MenuProcessor.ExceptionAsync("Select image failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
		
		if (string.IsNullOrEmpty(m_LocalPath))
			return;
		
		m_Image.URL  = true;
		m_Image.Path = m_LocalPath;
	}
}