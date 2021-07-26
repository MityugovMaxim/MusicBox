using UnityEngine;

public class UITapHandle : UIHandle
{
	bool           m_Interactable;
	bool           m_Processed;
	UITapIndicator m_Indicator;

	public void Setup(UITapIndicator _Indicator)
	{
		m_Indicator = _Indicator;
	}

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
			ProcessFail(0);
		
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
		float progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		ProcessSuccess(progress);
	}

	public override void TouchUp(int _ID, Rect _Area) { }

	public override void TouchMove(int _ID, Rect _Area) { }

	void ProcessSuccess(float _Progress)
	{
		if (m_Indicator != null)
			m_Indicator.Success(_Progress);
	}

	void ProcessFail(float _Progress)
	{
		if (m_Indicator != null)
			m_Indicator.Fail(_Progress);
	}
}