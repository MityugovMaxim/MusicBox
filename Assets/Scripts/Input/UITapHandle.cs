using UnityEngine;

public class UITapHandle : UIHandle
{
	public override float Progress => m_Progress;

	bool  m_Interactable;
	bool  m_Processed;
	float m_Progress;

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

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		float distance = Mathf.Abs(area.center.y - rect.center.y);
		float length   = (rect.height + area.height) * 0.5f;
		
		m_Progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		InvokeSuccess();
	}

	public override void TouchUp(int _ID, Rect _Area) { }

	public override void TouchMove(int _ID, Rect _Area) { }
}