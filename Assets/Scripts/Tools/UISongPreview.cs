using UnityEngine;
using UnityEngine.UI;

public class UISongPreview : UISongEntity
{
	[SerializeField] GameObject m_Background;
	[SerializeField] Button     m_PlayButton;
	[SerializeField] Button     m_StopButton;
	[SerializeField] UIGroup    m_ControlGroup;
	[SerializeField] UIGroup    m_LoaderGroup;
	[SerializeField] bool       m_Control;
	[SerializeField] bool       m_AutoPlay;

	protected override void Awake()
	{
		base.Awake();
		
		m_PlayButton.Subscribe(Play);
		m_StopButton.Subscribe(Stop);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_PlayButton.Unsubscribe(Play);
		m_StopButton.Unsubscribe(Stop);
		
		Stop();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		SongsManager.SubscribeState(ProcessState);
		SongsManager.SubscribeTrack(ProcessState);
		
		ProcessState();
	}

	protected override void OnDisable()
	{
		Stop();
		
		SongsManager.UnsubscribeState(ProcessState);
		SongsManager.UnsubscribeTrack(ProcessState);
		
		base.OnDisable();
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData()
	{
		if (m_Background != null)
			m_Background.SetActive(m_Control);
		
		if (m_AutoPlay)
			Play();
		else
			ProcessState();
	}

	void ProcessState()
	{
		string            songID = SongsManager.GetID();
		AudioChannelState state  = SongID == songID ? SongsManager.GetState() : AudioChannelState.Stop;
		
		if (state == AudioChannelState.Loading)
		{
			if (m_LoaderGroup != null)
				m_LoaderGroup.Show();
			
			if (m_ControlGroup != null)
				m_ControlGroup.Hide();
		}
		else
		{
			if (m_ControlGroup != null && m_Control)
				m_ControlGroup.Show();
			
			if (m_LoaderGroup != null)
				m_LoaderGroup.Hide();
			
			if (m_PlayButton != null && m_Control)
				m_PlayButton.gameObject.SetActive(state == AudioChannelState.Stop || state == AudioChannelState.Pause);
			
			if (m_StopButton != null && m_Control)
				m_StopButton.gameObject.SetActive(state == AudioChannelState.Play);
		}
	}

	void Play() => SongsManager.Play(SongID);

	void Stop()
	{
		if (SongsManager.GetID() == SongID)
			SongsManager.Stop();
	}
}
