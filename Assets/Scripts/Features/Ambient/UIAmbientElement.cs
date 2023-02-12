using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UIAmbientElement : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UIAmbientElement> { }

	[SerializeField] UIGroup m_ContentGroup;
	[SerializeField] UIGroup m_LoaderGroup;
	[SerializeField] Button  m_PreviousButton;
	[SerializeField] Button  m_PlayButton;
	[SerializeField] Button  m_PauseButton;
	[SerializeField] Button  m_NextButton;

	[Inject] AmbientManager m_AmbientManager;

	protected override void Awake()
	{
		base.Awake();
		
		m_PreviousButton.Subscribe(Previous);
		m_NextButton.Subscribe(Next);
		m_PlayButton.Subscribe(Play);
		m_PauseButton.Subscribe(Pause);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_PreviousButton.Unsubscribe(Previous);
		m_NextButton.Unsubscribe(Next);
		m_PlayButton.Unsubscribe(Play);
		m_PauseButton.Unsubscribe(Pause);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessState();
		
		m_AmbientManager.SubscribeState(ProcessState);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_AmbientManager.UnsubscribeState(ProcessState);
	}

	void Previous() => m_AmbientManager.Previous();

	void Next() => m_AmbientManager.Next();

	void Play() => m_AmbientManager.Play();

	void Pause() => m_AmbientManager.Pause();

	void ProcessState()
	{
		if (!IsActiveSelf)
			return;
		
		AudioChannelState state = m_AmbientManager.GetState();
		
		if (state == AudioChannelState.Loading)
		{
			m_LoaderGroup.Show();
			m_ContentGroup.Hide();
		}
		else
		{
			m_LoaderGroup.Hide();
			m_ContentGroup.Show();
			m_PauseButton.gameObject.SetActive(state == AudioChannelState.Play);
			m_PlayButton.gameObject.SetActive(state == AudioChannelState.Pause || state == AudioChannelState.Stop);
		}
	}
}
