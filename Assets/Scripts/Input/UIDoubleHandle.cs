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
		
		ProcessFail(0);
		
		m_Processed = true;
		m_Count     = 0;
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
		
		Rect rect = RectTransform.rect;
		Rect area = GetLocalRect(_Area);
		
		float position = area.center.y + m_Indicator.InputOffset;
		float distance = Mathf.Abs(position - rect.center.y);
		float length   = rect.height;
		float progress = 1.0f - Mathf.Max(distance - m_Indicator.InputError) / length;
		
		m_Processed = true;
		
		ProcessSuccess(progress);
	}

	public override void TouchUp(int _ID, Rect _Area)
	{
		if (m_Processed)
			return;
		
		m_Count--;
	}

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