using UnityEngine;
using Zenject;

public class UIFrameAction : UIFrameEntity
{
	static bool Processing { get; set; }

	[SerializeField] UIButton m_Button;

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

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }

	async void ProcessAction()
	{
		if (Processing)
			return;
		
		Processing = true;
		
		bool success = await FramesManager.SelectAsync(FrameID);
		
		Processing = false;
		
		if (success)
			return;
		
		await m_MenuProcessor.RetryAsync("frame_select", ProcessAction);
	}
}
