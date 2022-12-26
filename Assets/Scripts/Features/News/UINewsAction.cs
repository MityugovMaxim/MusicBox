using UnityEngine;
using Zenject;

public class UINewsAction : UINewsEntity
{
	[SerializeField] UIOverlayButton m_Button;

	[Inject] UrlProcessor  m_UrlProcessor;
	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(Process);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(Process);
	}

	async void Process()
	{
		string url = NewsManager.GetURL(NewsID);
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_UrlProcessor.ProcessURL(url);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }
}
