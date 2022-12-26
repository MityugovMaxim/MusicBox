using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using Facebook.MiniJSON;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.ColorMenu)]
public class UIColorMenu : UIMenu
{
	string ColorSchemeKey => $"COLOR_SCHEME_{m_SongID}";

	Color BackgroundPrimary
	{
		get => m_BackgroundPrimary.color;
		set => m_BackgroundPrimary.color = value;
	}

	Color BackgroundSecondary
	{
		get => m_BackgroundSecondary.color;
		set => m_BackgroundSecondary.color = value;
	}

	Color ForegroundPrimary
	{
		get => m_ForegroundPrimary.color;
		set => m_ForegroundPrimary.color = value;
	}

	Color ForegroundSecondary
	{
		get => m_ForegroundSecondary.color;
		set => m_ForegroundSecondary.color = value;
	}

	[SerializeField] Graphic         m_BackgroundPrimary;
	[SerializeField] Graphic         m_BackgroundSecondary;
	[SerializeField] Graphic         m_ForegroundPrimary;
	[SerializeField] Graphic         m_ForegroundSecondary;
	[SerializeField] UIColorsList    m_List;
	[SerializeField] Button          m_NextButton;
	[SerializeField] Button          m_PreviousButton;
	[SerializeField] UIOverlayToggle m_LibraryToggle;
	[SerializeField] UIOverlayToggle m_SchemeToggle;
	[SerializeField] Button          m_CancelButton;
	[SerializeField] Button          m_ConfirmButton;
	[SerializeField] Button          m_RefreshButton;

	ASFPlayer     m_Player;
	ASFColorTrack m_Track;
	ASFColorClip  m_Clip;
	string        m_SongID;

	readonly List<ColorsSnapshot> m_LibrarySnapshots = new List<ColorsSnapshot>();
	readonly List<ColorsSnapshot> m_SchemeSnapshots  = new List<ColorsSnapshot>();

	readonly Dictionary<ASFColorClip, ColorsSnapshot> m_Registry = new Dictionary<ASFColorClip, ColorsSnapshot>();

	protected override void Awake()
	{
		base.Awake();
		
		m_NextButton.onClick.AddListener(Next);
		m_PreviousButton.onClick.AddListener(Previous);
		m_CancelButton.onClick.AddListener(Cancel);
		m_ConfirmButton.onClick.AddListener(Confirm);
		m_RefreshButton.onClick.AddListener(Refresh);
		m_LibraryToggle.ValueChanged.AddListener(LibraryMode);
		m_SchemeToggle.ValueChanged.AddListener(SchemeMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_NextButton.onClick.RemoveListener(Next);
		m_PreviousButton.onClick.RemoveListener(Previous);
		m_CancelButton.onClick.RemoveListener(Cancel);
		m_ConfirmButton.onClick.RemoveListener(Confirm);
		m_RefreshButton.onClick.RemoveListener(Refresh);
		m_LibraryToggle.ValueChanged.RemoveListener(LibraryMode);
		m_SchemeToggle.ValueChanged.RemoveListener(SchemeMode);
	}

	public void Setup(string _SongID, ASFPlayer _Player)
	{
		m_SongID = _SongID;
		m_Player = _Player;
		m_Track  = m_Player.GetTrack<ASFColorTrack>();
		
		m_SchemeSnapshots.Clear();
		m_LibrarySnapshots.Clear();
		
		FetchLibrary();
		
		LoadScheme();
		
		ScanScheme();
		
		SchemeMode(true);
	}

	public void Select(ASFColorClip _Clip)
	{
		m_Clip = _Clip;
		
		BackgroundPrimary   = m_Clip.BackgroundPrimary;
		BackgroundSecondary = m_Clip.BackgroundSecondary;
		ForegroundPrimary   = m_Clip.ForegroundPrimary;
		ForegroundSecondary = m_Clip.ForegroundSecondary;
		
		Sample();
		
		SchemeMode(true);
	}

	void LibraryMode(bool _Value)
	{
		if (!_Value)
			return;
		
		m_LibraryToggle.SetState(true);
		m_SchemeToggle.SetState(false);
		
		m_List.Clear();
		
		m_List.Setup(m_LibrarySnapshots, SelectLibrary);
	}

	void SchemeMode(bool _Value)
	{
		if (!_Value)
			return;
		
		m_SchemeToggle.SetState(true);
		m_LibraryToggle.SetState(false);
		
		m_List.Clear();
		
		m_List.Setup(m_SchemeSnapshots, SelectScheme);
	}

	void SelectLibrary(ColorsSnapshot _Snapshot)
	{
	}

	void SelectScheme(ColorsSnapshot _Snapshot)
	{
		if (m_Clip == null)
			return;
		
		BackgroundPrimary   = _Snapshot.BackgroundPrimary;
		BackgroundSecondary = _Snapshot.BackgroundSecondary;
		ForegroundPrimary   = _Snapshot.ForegroundPrimary;
		ForegroundSecondary = _Snapshot.ForegroundSecondary;
		
		m_Registry[m_Clip] = _Snapshot;
	}

	protected override void OnShowStarted()
	{
		m_SchemeToggle.Value  = true;
		m_LibraryToggle.Value = false;
	}

	protected override void OnHideStarted()
	{
		SaveScheme();
	}

	void Next()
	{
		Select(1);
		
		if (m_Player != null && m_Clip != null)
			m_Player.Time = m_Clip.Time;
		
		Sample();
	}

	void Previous()
	{
		Select(-1);
		
		if (m_Player != null && m_Clip != null)
			m_Player.Time = m_Clip.Time;
		
		Sample();
	}

	void Select(int _Offset)
	{
		if (m_Track == null)
			return;
		
		m_Track.SortClips();
		
		int index = -1;
		
		for (int i = 0; i < m_Track.Clips.Count; i++)
		{
			if (m_Track.Clips[i] != m_Clip)
				continue;
			
			index = i;
			
			break;
		}
		
		if (index < 0)
			return;
		
		index = MathUtility.Repeat(index + _Offset, m_Track.Clips.Count);
		
		Select(m_Track.Clips[index]);
	}

	void Refresh()
	{
		if (m_Track == null || m_Track.Clips == null)
			return;
		
		foreach (ASFColorClip clip in m_Track.Clips)
		{
			if (clip == null || !m_Registry.TryGetValue(clip, out ColorsSnapshot snapshot) || snapshot == null)
				continue;
			
			clip.BackgroundPrimary   = snapshot.BackgroundPrimary;
			clip.BackgroundSecondary = snapshot.BackgroundSecondary;
			clip.ForegroundPrimary   = snapshot.ForegroundPrimary;
			clip.ForegroundSecondary = snapshot.ForegroundSecondary;
		}
		
		m_Player.Sample();
	}

	void Confirm()
	{
		if (m_Clip == null)
			return;
		
		m_Clip.BackgroundPrimary   = BackgroundPrimary;
		m_Clip.BackgroundSecondary = BackgroundSecondary;
		m_Clip.ForegroundPrimary   = ForegroundPrimary;
		m_Clip.ForegroundSecondary = ForegroundSecondary;
		
		m_Player.Sample();
		
		Hide();
	}

	void Cancel()
	{
		Hide();
	}

	void Sample()
	{
		IASFColorSampler sampler = m_Track.Context as IASFColorSampler;
		
		if (sampler == null)
			return;
		
		ASFColorClip clip = new ASFColorClip(
			0,
			BackgroundPrimary,
			BackgroundSecondary,
			ForegroundPrimary,
			ForegroundSecondary
		);
		
		sampler.Sample(clip, clip, 1);
	}

	void SaveScheme()
	{
		
	}

	async void FetchLibrary()
	{
		DatabaseReference data = FirebaseDatabase.DefaultInstance.RootReference.Child("colors");
		
		DataSnapshot snapshot = await data.GetValueAsync();
		
		m_LibrarySnapshots.AddRange(snapshot.Children.Select(_Data => new ColorsSnapshot(_Data)));
	}

	void LoadScheme()
	{
		if (!PlayerPrefs.HasKey(ColorSchemeKey))
			return;
		
		string json = PlayerPrefs.GetString(ColorSchemeKey);
		
		if (string.IsNullOrEmpty(json))
			return;
		
		Dictionary<string, object> data = Json.Deserialize(json) as Dictionary<string, object>;
		
		if (data == null)
			return;
		
		
	}

	void ScanScheme()
	{
	}
}
