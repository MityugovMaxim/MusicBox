using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SongCreateRequest : FunctionRequest<string>
{
	protected override string Command => "SongCreate";

	string Title  { get; }
	string Artist { get; }
	float  Speed  { get; }
	float  BPM    { get; }
	float  Bar    { get; }

	public SongCreateRequest(
		string _Title,
		string _Artist,
		float  _Speed,
		float  _BPM,
		float  _Bar
	)
	{
		Title  = _Title;
		Artist = _Artist;
		Speed  = _Speed;
		BPM    = _BPM;
		Bar    = _Bar;
	}

	protected override void Serialize(IDictionary<string, object> _Data)
	{
		_Data["title"]  = Title;
		_Data["artist"] = Artist;
		_Data["speed"]  = Speed;
		_Data["bpm"]    = BPM;
		_Data["bar"]    = Bar;
	}

	protected override string Success(object _Data)
	{
		return (string)_Data;
	}

	protected override string Fail()
	{
		return null;
	}
}

[Menu(MenuType.SongCreateMenu)]
public class UISongCreateMenu : UIMenu
{
	UISongCreateMenuPage[] Pages => new UISongCreateMenuPage[]
	{
		m_LabelsPage,
		m_MusicPage,
		m_PreviewPage,
		m_RhythmPage,
		m_ArtworkPage,
	};

	int Page { get; set; }

	[SerializeField] UISongCreateLabelsPage  m_LabelsPage;
	[SerializeField] UISongCreateMusicPage   m_MusicPage;
	[SerializeField] UISongCreatePreviewPage m_PreviewPage;
	[SerializeField] UISongCreateRhythmPage  m_RhythmPage;
	[SerializeField] UISongCreateArtworkPage m_ArtworkPage;

	[SerializeField] UIGroup m_ErrorGroup;
	[SerializeField] Button  m_NextButton;
	[SerializeField] Button  m_PreviousButton;
	[SerializeField] Button  m_ExecuteButton;
	[SerializeField] Button  m_CancelButton;

	[Inject] SocialProcessor m_SocialProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	string m_SongID;
	string m_UserID;

	protected override void Awake()
	{
		base.Awake();
		
		m_NextButton.Subscribe(Next);
		m_PreviousButton.Subscribe(Previous);
		m_ExecuteButton.Subscribe(Execute);
		m_CancelButton.Subscribe(Cancel);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_NextButton.Unsubscribe(Next);
		m_PreviousButton.Unsubscribe(Previous);
		m_ExecuteButton.Unsubscribe(Execute);
		m_CancelButton.Unsubscribe(Cancel);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Select(0);
	}

	void Next()
	{
		Select(Page + 1);
	}

	void Previous()
	{
		Select(Page - 1);
	}

	async void Execute()
	{
		m_UserID = m_SocialProcessor.UserID;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		try
		{
			await CreateSong();
			
			await UploadMusic();
			
			await UploadPreview();
			
			await UploadArtwork();
		}
		catch (Exception exception)
		{
			await m_MenuProcessor.ExceptionAsync(exception);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void Cancel()
	{
		Hide();
	}

	async Task CreateSong()
	{
		if (!string.IsNullOrEmpty(m_SongID))
		{
			Select(m_LabelsPage);
			
			throw new UnityException("Create song failed");
		}
		
		UIProcessingMenu processingMenu = m_MenuProcessor.GetMenu<UIProcessingMenu>();
		
		processingMenu.Text = "Creating song...";
		
		SongCreateRequest request = new SongCreateRequest(
			m_LabelsPage.Title,
			m_LabelsPage.Artist,
			m_RhythmPage.Speed,
			m_RhythmPage.BPM,
			m_RhythmPage.Bar
		);
		
		m_SongID = await request.SendAsync();
	}

	async Task UploadMusic()
	{
		if (string.IsNullOrEmpty(m_SongID))
		{
			Select(m_LabelsPage);
			
			throw new UnityException("Upload music failed. Song ID is invalid.");
		}
		
		if (string.IsNullOrEmpty(m_UserID))
		{
			Select(m_LabelsPage);
			
			throw new UnityException("Upload music failed. User ID is invalid.");
		}
		
		string path = $"Maps/{m_UserID}/Music/{m_SongID}.ogg";
		
		UIProcessingMenu processingMenu = m_MenuProcessor.GetMenu<UIProcessingMenu>();
		
		processingMenu.Text = "Compressing music...";
		
		string file = await m_MusicPage.CreateMusic(_Progress => processingMenu.SetProgress("Compressing music...", _Progress));
		
		processingMenu.Text = "Uploading music...";
		
		bool success = await UploadFile(file, path, "audio/ogg", _Progress => processingMenu.SetProgress("Uploading music...", _Progress));
		
		if (success)
			return;
		
		Select(m_MusicPage);
		
		throw new UnityException("Upload music failed");
	}

	async Task UploadPreview()
	{
		if (string.IsNullOrEmpty(m_SongID) || string.IsNullOrEmpty(m_UserID))
		{
			Select(m_LabelsPage);
			
			throw new UnityException("Upload preview failed");
		}
		
		string path = $"Maps/{m_UserID}/Preview/{m_SongID}.ogg";
		
		UIProcessingMenu processingMenu = m_MenuProcessor.GetMenu<UIProcessingMenu>();
		
		processingMenu.Text = "Compressing preview...";
		
		string file = await m_PreviewPage.CreatePreview();
		
		processingMenu.Text = "Uploading preview...";
		
		bool success = await UploadFile(file, path, "audio/ogg", _Progress => processingMenu.SetProgress("Uploading preview...", _Progress));
		
		if (success)
			return;
		
		Select(m_PreviewPage);
		
		throw new UnityException("Upload preview failed");
	}

	async Task UploadArtwork()
	{
		if (string.IsNullOrEmpty(m_SongID) || string.IsNullOrEmpty(m_UserID))
		{
			Select(m_LabelsPage);
			
			throw new UnityException("Upload artwork failed");
		}
		
		string path = $"Maps/{m_UserID}/Artwork/{m_SongID}.jpg";
		
		UIProcessingMenu processingMenu = m_MenuProcessor.GetMenu<UIProcessingMenu>();
		
		processingMenu.Text = "Compressing artwork...";
		
		byte[] artwork = m_ArtworkPage.CreateArtwork();
		
		processingMenu.Text = "Uploading artwork...";
		
		bool success = await UploadFile(path, artwork, "image/jpeg");
		
		if (success)
			return;
		
		Select(m_ArtworkPage);
		
		throw new UnityException("Upload artwork failed");
	}

	static async Task<bool> UploadFile(string _File, string _Path, string _ContentType, Action<float> _Progress)
	{
		if (string.IsNullOrEmpty(_File) || string.IsNullOrEmpty(_Path))
			return false;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_Path);
		
		MetadataChange metadata = new MetadataChange();
		metadata.ContentType = _ContentType;
		
		await reference.PutFileAsync(_File, metadata);
		
		return true;
	}

	static async Task<bool> UploadFile(string _Path, byte[] _Data, string _ContentType)
	{
		if (string.IsNullOrEmpty(_Path))
			return false;
		
		if (_Data == null || _Data.Length == 0)
			return false;
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(_Path);
		
		MetadataChange metadata = new MetadataChange();
		metadata.ContentType = _ContentType;
		
		await reference.PutBytesAsync(_Data, metadata);
		
		return true;
	}

	void Select(UISongCreateMenuPage _Page)
	{
		Select(Pages != null && _Page != null ? Array.IndexOf(Pages, _Page) : 0);
	}

	void Select(int _Page)
	{
		if (Pages.Length == 0)
			return;
		
		if (Page < _Page && !Pages[Page].Valid)
		{
			Error();
			return;
		}
		
		Page = Mathf.Clamp(_Page, 0, Pages.Length - 1);
		
		for (int i = 0; i < Pages.Length; i++)
		{
			if (Page == i)
				Pages[i].Show();
			else
				Pages[i].Hide();
		}
		
		ProcessControl();
	}

	async void Error()
	{
		await m_ErrorGroup.ShowAsync();
		
		await m_ErrorGroup.HideAsync();
	}

	void ProcessControl()
	{
		m_PreviousButton.interactable = Page > 0;
		
		m_NextButton.interactable = Page < Pages.Length - 1;
		
		m_ExecuteButton.interactable = Page >= Pages.Length - 1;
	}
}

public abstract class UISongCreateMenuPage : UIGroup
{
	public abstract bool Valid { get; }
}
