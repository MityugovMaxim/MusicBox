using UnityEngine;
using Zenject;

public class UIProfileFrame : UIEntity
{
	[SerializeField] UIFrameImage m_Frame;

	[Inject] ProfileFrameParameter m_ProfileFrame;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessFrame(m_ProfileFrame.Value);
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unsubscribe();
	}

	void Subscribe() => m_ProfileFrame.Subscribe(ProcessFrame);

	void Unsubscribe() => m_ProfileFrame.Unsubscribe(ProcessFrame);

	void ProcessFrame(string _FrameID) => m_Frame.FrameID = _FrameID;
}