using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCounter
{
	const float CLICK_THRESHOLD = 0.3f;
	const float CLICK_DISTANCE  = 40;

	readonly int m_ClickTarget;

	int     m_ClickID;
	int     m_ClickCount;
	Vector2 m_ClickPosition;
	float   m_ClickTime;

	public ClickCounter(int _ClickTarget)
	{
		m_ClickTarget = _ClickTarget;
	}

	public bool Execute(PointerEventData _EventData)
	{
		return Execute(_EventData.pointerId, _EventData.clickTime, _EventData.position);
	}

	public bool Execute(int _ClickID, float _ClickTime, Vector2 _ClickPosition)
	{
		float distance = Vector2.Distance(_ClickPosition, m_ClickPosition);
		
		if (_ClickID == m_ClickID && _ClickTime - m_ClickTime <= CLICK_THRESHOLD && distance < CLICK_DISTANCE)
		{
			m_ClickCount++;
			m_ClickID       = _ClickID;
			m_ClickTime     = _ClickTime;
			m_ClickPosition = _ClickPosition;
		}
		else
		{
			m_ClickCount    = 1;
			m_ClickID       = _ClickID;
			m_ClickTime     = _ClickTime;
			m_ClickPosition = _ClickPosition;
		}
		
		if (m_ClickCount < m_ClickTarget)
			return false;
		
		m_ClickCount = 0;
		m_ClickTime  = 0;
		
		return true;
	}
}