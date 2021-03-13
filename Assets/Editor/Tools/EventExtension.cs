using UnityEngine;

public static class EventExtension
{
	static Vector2 m_Origin;

	public static void SetPosition(this Event _Event, float _Position)
	{
		m_Origin = new Vector2(_Position, _Position) - _Event.mousePosition;
	}

	public static void SetPosition(this Event _Event, Vector2 _Position)
	{
		m_Origin = _Position - _Event.mousePosition;
	}

	public static void SetPosition(this Event _Event, Rect _Rect)
	{
		m_Origin = _Rect.position - _Event.mousePosition;
	}

	public static Vector2 GetPosition(this Event _Event)
	{
		return m_Origin + _Event.mousePosition;
	}

	public static float GetHorizontalPosition(this Event _Event)
	{
		return m_Origin.x + _Event.mousePosition.x;
	}

	public static float GetVerticalPosition(this Event _Event)
	{
		return m_Origin.y + _Event.mousePosition.y;
	}
}