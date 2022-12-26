using UnityEngine;

public class VerticalStackLayout : Layout
{
	Vector2 Position { get; }
	float   Width    { get; }
	float   Spacing  { get; }
	Vector2 Pivot    { get; }

	float m_Height;

	public VerticalStackLayout(
		Vector2 _Position,
		float   _Width,
		float   _Spacing,
		Vector2 _Pivot
	)
	{
		Position = _Position;
		Width    = _Width;
		Spacing  = _Spacing;
		Pivot    = _Pivot;
	}

	public static void Start(UILayout _Layout, float _Spacing)
	{
		if (_Layout == null)
			return;
		
		_Layout.EndLayout();
		
		Vector2 pivot = _Layout.RectTransform.pivot;
		
		float direction = pivot.y * 2 - 1;
		
		Rect rect = _Layout.GetLocalRect();
		
		VerticalStackLayout layout = new VerticalStackLayout(
			new Vector2(0, -rect.height * direction),
			rect.width,
			_Spacing,
			pivot
		);
		
		_Layout.StartLayout(layout);
	}

	public static void End(UILayout _Layout)
	{
		if (_Layout != null)
			_Layout.EndLayout();
	}

	public override Vector2 GetSize() => new Vector2(Width, Mathf.Max(0, m_Height - Spacing));

	public override Rect GetRect(Vector2 _Size)
	{
		float direction = Pivot.y * 2 - 1;
		
		Rect rect = new Rect(
			Position.x,
			Position.y - m_Height * direction + _Size.y * (1.0f - Pivot.y),
			Width,
			_Size.y
		);
		
		m_Height += _Size.y + Spacing;
		
		return rect;
	}
}
