using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongCreateArtworkPage : UISongCreateMenuPage
{
	public override bool Valid => m_Artwork != null;

	[SerializeField] RawImage m_Image;
	[SerializeField] UIGroup  m_ArtworkGroup;
	[SerializeField] Button   m_SelectButton;

	[Inject] IFileManager  m_FileManager;
	[Inject] MenuProcessor m_MenuProcessor;

	Texture2D m_Artwork;

	protected override void Awake()
	{
		base.Awake();
		
		m_SelectButton.Subscribe(Select);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SelectButton.Unsubscribe(Select);
	}

	public byte[] CreateArtwork()
	{
		if (m_Artwork == null)
			return null;
		
		Texture2D artwork = m_Artwork.SetSize(512);
		
		return artwork.EncodeToJPG();
	}

	async void Select()
	{
		string path = null;
		
		try
		{
			path = await m_FileManager.SelectFile(FileManagerUtility.ImageExtensions);
		}
		catch (TaskCanceledException) { }
		catch (OperationCanceledException) { }
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		if (string.IsNullOrEmpty(path))
			return;
		
		await m_ArtworkGroup.HideAsync();
		
		m_Artwork = await WebRequest.LoadTextureFile(path);
		
		if (m_Artwork == null)
			return;
		
		m_Artwork = m_Artwork.SetSize(512, 512);
		
		m_Image.texture = m_Artwork;
		
		m_ArtworkGroup.Show();
	}
}