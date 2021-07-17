using UnityEngine;

public class UIDoubleHandle : UIHandle
{
	const int MIN_COUNT = 2;

	public override float Progress => m_Progress;

	bool  m_Interactable;
	bool  m_Processed;
	int   m_Count;
	float m_Progress;

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

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Count++;
		
		if (m_Count < MIN_COUNT)
			return;
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		float distance = Mathf.Abs(area.center.y - rect.center.y);
		float length   = (rect.height + area.height) * 0.5f;
		
		m_Progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		InvokeSuccess();
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Count--;
	}

	public override void TouchMove(int _ID, Rect _Area) { }
}