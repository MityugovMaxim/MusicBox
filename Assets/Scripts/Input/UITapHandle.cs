using UnityEngine;

public class UITapHandle : UIHandle
{
	protected override bool Processed => m_Processed;

	[SerializeField] UITapIndicator m_Indicator;

	bool m_Processed;

	public override void EnterZone() { }

	public override void ExitZone()
	{
		if (m_Processed)
			return;
		
		m_Processed = true;
		
		m_Indicator.Fail();
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
		
		float position = area.center.y;
		float distance = Mathf.Abs(position - rect.center.y);
		float length   = area.height;
		float progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		m_Indicator.Success(progress);
	}

	public override void TouchUp(int _ID, Rect _Area) { }

	public override void TouchMove(int _ID, Rect _Area) { }
}