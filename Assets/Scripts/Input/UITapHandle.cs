using UnityEngine;

public class UITapHandle : UIHandle
{
	const float FRAME_ERROR = 20;

	protected override bool Processed => m_Processed;

	[SerializeField] UITapIndicator m_Indicator;

	bool m_Processed;

	public override void EnterZone() { }

	public override void ExitZone()
	{
		if (m_Processed)
			return;
		
		ProcessFail(0);
		
		m_Processed = true;
	}

	public override void Reverse() { }

	public override void Restore()
	{
		m_Processed = false;
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		float distance = Mathf.Abs(area.center.y - rect.center.y);
		float length   = rect.height + FRAME_ERROR;
		float progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		ProcessSuccess(progress);
	}

	public override void TouchUp(int _ID, Rect _Area) { }

	public override void TouchMove(int _ID, Rect _Area) { }

	void ProcessSuccess(float _Progress)
	{
		m_Indicator.Success(_Progress);
	}

	void ProcessFail(float _Progress)
	{
		m_Indicator.Fail(_Progress);
	}
}