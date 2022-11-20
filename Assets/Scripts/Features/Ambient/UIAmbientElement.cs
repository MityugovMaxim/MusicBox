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
		
		ProcessControl();
		
		m_AmbientManager.SubscribePlay(ProcessControl);
		m_AmbientManager.SubscribePause(ProcessControl);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_AmbientManager.UnsubscribePlay(ProcessControl);
		m_AmbientManager.UnsubscribePause(ProcessControl);
	}

	void Previous()
	{
		m_AmbientManager.Previous();
	}

	void Next()
	{
		m_AmbientManager.Next();
	}

	void Play()
	{
		m_AmbientManager.Play();
	}

	void Pause()
	{
		m_AmbientManager.Pause();
	}

	async void ProcessControl()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		int frame = Time.frameCount;
		
		await m_AmbientManager.Preload();
		
		bool instant = frame == Time.frameCount;
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
		
		m_PauseButton.gameObject.SetActive(m_AmbientManager.Playing);
		m_PlayButton.gameObject.SetActive(m_AmbientManager.Paused);
	}
}
