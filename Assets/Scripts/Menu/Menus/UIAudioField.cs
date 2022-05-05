using System;
using System.IO;
using System.Threading.Tasks;
using AudioBox.Logging;
using TMPro;
using UnityEngine;
using Zenject;

public class UIAudioField : UIEntity
{
	public string Label
	{
		get => m_Label.text;
		set => m_Label.text = value;
	}

	[SerializeField] TMP_Text    m_Label;
	[SerializeField] TMP_Text    m_File;
	[SerializeField] AudioSource m_Source;
	[SerializeField] GameObject  m_Icon;

	[Inject] IFileManager     m_FileManager;
	[Inject] StorageProcessor m_StorageProcessor;
	[Inject] MenuProcessor    m_MenuProcessor;

	string m_RemotePath;
	string m_LocalPath;

	public async void Setup(string _Path)
	{
		m_Icon.SetActive(false);
		
		m_RemotePath = _Path;
		
		m_Source.clip = await m_StorageProcessor.LoadAudioClipAsync(m_RemotePath);
		
		m_File.text = !string.IsNullOrEmpty(m_RemotePath)
			? Path.GetFileNameWithoutExtension(m_RemotePath)
			: "Unnamed";
		
		m_Icon.SetActive(m_Source.clip != null);
	}

	public void Play()
	{
		if (m_Source != null && m_Source.clip != null)
			m_Source.Play();
	}

	public void Stop()
	{
		if (m_Source != null && m_Source.clip != null)
			m_Source.Stop();
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
			Log.Exception(this, exception, "Upload audio failed. Remote path: {0} Local path: {1}.", m_RemotePath, m_LocalPath);
			
			await m_MenuProcessor.ExceptionAsync("Upload audio failed.", exception);
		}
		
		m_Source.clip = await m_StorageProcessor.LoadAudioClipAsync(m_RemotePath);
		
		m_Icon.SetActive(m_Source.clip != null);
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	public async void Select()
	{
		m_LocalPath = null;
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		try
		{
			m_LocalPath = await m_FileManager.SelectFile("ogg");
		}
		catch (TaskCanceledException) { }
		catch (Exception exception)
		{
			Log.Exception(this, exception, "Select audio file failed.");
			
			await m_MenuProcessor.ExceptionAsync("Select audio failed", exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
		
		if (string.IsNullOrEmpty(m_LocalPath))
			return;
		
		m_Source.clip = await WebRequest.LoadAudioClipFile(m_LocalPath);
		
		m_Icon.SetActive(m_Source.clip != null);
	}
}