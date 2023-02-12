using UnityEngine;

public class UIFrameCheck : UIFrameEntity
{
	[SerializeField] UIGroup m_CheckGroup;

	protected override void Subscribe()
	{
		FramesManager.Profile.Subscribe(ProcessCheck);
	}

	protected override void Unsubscribe()
	{
		FramesManager.Profile.Unsubscribe(ProcessCheck);
	}

	protected override void ProcessData()
	{
		string frameID = FramesManager.GetFrameID();
		if (FrameID == frameID)
			m_CheckGroup.Show(true);
		else
			m_CheckGroup.Hide(true);
	}

	void ProcessCheck()
	{
		string frameID = FramesManager.GetFrameID();
		if (FrameID == frameID)
			m_CheckGroup.Show();
		else
			m_CheckGroup.Hide();
	}
}