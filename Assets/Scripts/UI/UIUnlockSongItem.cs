using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIUnlockSongItem : UIUnlockItem
{
	[Preserve]
	public class Pool : UIEntityPool<UIUnlockSongItem> { }

	SongPreview Preview
	{
		get => m_Preview;
		set
		{
			if (m_Preview == value)
				return;
			
			if (m_Preview != null)
			{
				m_Preview.OnPlay -= OnPlay;
				m_Preview.OnStop -= OnStop;
			}
			
			m_Preview = value;
			
			if (m_Preview != null)
			{
				m_Preview.OnPlay += OnPlay;
				m_Preview.OnStop += OnStop;
			}
		}
	}

	[SerializeField] UISongImage m_Image;
	[SerializeField] UISongLabel m_Label;
	[SerializeField] UIGroup     m_PlayGroup;
	[SerializeField] UIGroup     m_StopGroup;
	[SerializeField] Button      m_PlayButton;
	[SerializeField] Button      m_StopButton;

	string      m_SongID;
	SongPreview m_Preview;

	protected override void Awake()
	{
		base.Awake();
		
		m_PlayButton.onClick.AddListener(Play);
		m_StopButton.onClick.AddListener(Stop);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_PlayButton.onClick.RemoveListener(Play);
		m_StopButton.onClick.RemoveListener(Stop);
		
		Preview = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Preview = null;
	}

	public void Setup(string _SongID, SongPreview _Preview)
	{
		m_SongID = _SongID;
		
		m_Image.Setup(m_SongID);
		m_Label.Setup(m_SongID);
		
		Preview = _Preview;
		
		m_PlayGroup.Show(true);
		m_StopGroup.Hide(true);
		
		Restore();
	}

	void Play()
	{
		Preview.Play(m_SongID);
	}

	void Stop()
	{
		m_Preview.Stop();
	}

	void OnPlay(string _SongID)
	{
		if (m_SongID == _SongID)
		{
			m_StopGroup.Show();
			m_PlayGroup.Hide();
		}
		else
		{
			m_PlayGroup.Show();
			m_StopGroup.Hide();
		}
	}

	void OnStop(string _SongID)
	{
		if (m_SongID != _SongID)
			return;
		
		m_PlayGroup.Show();
		m_StopGroup.Hide();
	}
}