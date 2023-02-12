using UnityEngine;
using Zenject;

public class UISongAction : UISongEntity
{
	[SerializeField] UIOverlayButton m_Button;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(ProcessAction);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(ProcessAction);
	}

	void ProcessAction()
	{
		UISongMenu songMenu = m_MenuProcessor.GetMenu<UISongMenu>();
		
		if (songMenu == null)
			return;
		
		SongsManager.Stop();
		
		songMenu.Setup(SongID);
		songMenu.Show();
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }
}
