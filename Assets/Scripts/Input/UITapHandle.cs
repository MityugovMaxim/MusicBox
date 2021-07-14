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

	public override void TouchDown(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Processed = true;
		
		InvokeSuccess();
	}

	public override void TouchUp(int _ID, Vector2 _Position) { }

	public override void TouchMove(int _ID, Vector2 _Position) { }
}