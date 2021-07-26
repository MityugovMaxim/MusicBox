using UnityEngine;

public class UIDoubleHandle : UIHandle
{
	const int MIN_COUNT = 2;

	UIDoubleIndicator m_Indicator;
	bool              m_Interactable;
	bool              m_Processed;
	int               m_Count;

	public void Setup(UIDoubleIndicator _Indicator)
	{
		m_Indicator = _Indicator;
	}

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
			ProcessFail(0);
		
		m_Interactable = false;
		m_Processed    = false;
		m_Count        = 0;
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		#if UNITY_EDITOR
		m_Count += 2;
		#else
		m_Count++;
		#endif
		
		if (m_Count < MIN_COUNT)
			return;
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		float distance = Mathf.Abs(area.center.y - rect.center.y);
		float length   = (rect.height + area.height) * 0.5f;
		float progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		ProcessSuccess(progress);
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (!m_Interactable || m_Processed)
			return;
		
		m_Count--;
	}

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