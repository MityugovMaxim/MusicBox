using UnityEngine;
using Zenject;

public class UIFrameItem : UIEntity
{
	[SerializeField] UIFrameImage m_Image;

	[Inject] FramesManager m_FramesManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessData();
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unsubscribe();
	}

	void Subscribe()
	{
		m_FramesManager.Profile.Subscribe(ProcessData);
	}

	void Unsubscribe()
	{
		m_FramesManager.Profile.Unsubscribe(ProcessData);
	}

	async void ProcessData()
	{
		await m_FramesManager.Activate();
		
		m_Image.FrameID = m_FramesManager.GetFrameID();
	}
}
