using UnityEngine;

public class UIDoubleHandle : UIHandle
{
	const int MIN_COUNT = 2;

	protected override bool Processed => m_Processed;

	[SerializeField] UIDoubleIndicator m_Indicator;

	bool m_Processed;
	int  m_Count;

	public override void EnterZone()
	{
		if (m_Processed)
			return;
		
		m_Count = 0;
	}

	public override void ExitZone()
	{
		if (m_Processed)
			return;
		
		m_Processed = true;
		m_Count     = 0;
		
		m_Indicator.Fail(0);
	}

	public override void Reverse()
	{
		if (m_Processed)
			return;
		
		m_Count = 0;
	}

	public override void Restore()
	{
		m_Processed = false;
		m_Count     = 0;
	}

	public override void TouchDown(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		#if UNITY_EDITOR
		m_Count += 2;
		#else
		m_Count++;
		#endif
		
		if (m_Count < MIN_COUNT)
			return;
		
		Rect area = GetLocalRect(_Area);
		Rect rect = RectTransform.rect;
		
		float position = area.center.y;
		float distance = Mathf.Abs(position - rect.center.y);
		float length   = area.height;
		float progress = 1.0f - distance / length;
		
		m_Processed = true;
		
		m_Indicator.Success(progress);
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		m_Count--;
	}

	public override void TouchMove(int _ID, Rect _Area) { }
}