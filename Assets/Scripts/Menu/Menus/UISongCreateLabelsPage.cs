using UnityEngine;

public class UISongCreateLabelsPage : UISongCreateMenuPage
{
	public override bool Valid
	{
		get
		{
			if (string.IsNullOrEmpty(Title))
				return false;
			
			if (string.IsNullOrEmpty(Artist))
				return false;
			
			return true;
		}
	}

	public string Title  { get; private set; }
	public string Artist { get; private set; }

	[SerializeField] UIStringField m_TitleField;
	[SerializeField] UIStringField m_ArtistField;

	[SerializeField] UIGroup m_TitleErrorGroup;
	[SerializeField] UIGroup m_ArtistErrorGroup;

	protected override void Awake()
	{
		base.Awake();
		
		m_TitleField.Setup(this, nameof(Title));
		m_ArtistField.Setup(this, nameof(Artist));
		
		m_TitleField.OnSubmit  += SubmitTitle;
		m_ArtistField.OnSubmit += SubmitArtist;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TitleField.OnSubmit  -= SubmitTitle;
		m_ArtistField.OnSubmit -= SubmitArtist;
	}

	void SubmitTitle(string _Title)
	{
		m_TitleErrorGroup.Hide();
		
		Title = _Title;
		
		if (string.IsNullOrEmpty(Title))
			m_TitleErrorGroup.Show();
	}

	void SubmitArtist(string _Artist)
	{
		m_ArtistErrorGroup.Hide();
		
		Artist = _Artist;
		
		if (string.IsNullOrEmpty(Artist))
			m_ArtistErrorGroup.Show();
	}
}