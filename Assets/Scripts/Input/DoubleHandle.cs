using UnityEngine;

public class DoubleHandle : UIHandle
{
	const int MIN_COUNT = 2;

	bool m_Interactable;
	bool m_Processed;
	int  m_Count;

	public override void StartReceiveInput()
	{
		if (m_Interactable)
			return;
		
		m_Interactable = true;
		m_Processed    = false;
		m_Count        = 0;
	}

	public override void StopReceiveInput()
	{
		if (!m_Interactable)
			return;
		
		if (!m_Processed)
			InvokeFail();
		
		m_Interactable = false;
		m_Processed    = false;
		m_Count        = 0;
	}

	public override void TouchDown(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Count++;
		
		if (m_Count < MIN_COUNT)
			return;
		
		m_Processed = true;
		
		InvokeSuccess();
	}

	public override void TouchUp(int _ID, Vector2 _Position)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Count--;
	}

	public override void TouchMove(int _ID, Vector2 _Position) { }
}