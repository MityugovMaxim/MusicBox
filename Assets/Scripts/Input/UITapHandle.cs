using UnityEngine;

public class UITapHandle : UIHandle
{
	bool m_Interactable;
	bool m_Processed;

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
	}

	public override void StopReceiveInput()
	{
		if (!m_Interactable)
			return;
		
		if (!m_Processed)
			InvokeFail();
		
		m_Interactable = false;
		m_Processed    = false;
	}

	public override void TouchDown(Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Interactable = false;
		m_Processed    = true;
		
		InvokeSuccess();
	}

	public override void TouchUp(Vector2 _Position) { }

	public override void TouchMove(Vector2 _Position) { }
}