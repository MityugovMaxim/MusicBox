using UnityEngine;

public abstract class Layout
{
	public float GetWidth() => GetSize().x;
	public float GetHeight() => GetSize().y;
	public abstract Vector2 GetSize();
	public abstract Rect GetRect(Vector2 _Size);
}