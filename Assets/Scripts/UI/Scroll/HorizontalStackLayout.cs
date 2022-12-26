using UnityEngine;

public class HorizontalStackLayout : Layout
{
	Vector2 Position { get; }
	float   Height    { get; }
	float   Spacing  { get; }
	Vector2 Pivot    { get; }

	float m_Width;

	public HorizontalStackLayout(
		Vector2 _Position,
		float   _Height,
		float   _Spacing,
		Vector2 _Pivot
	)
	{
		Position = _Position;
		Height   = _Height;
		Spacing  = _Spacing;
		Pivot    = _Pivot;
	}

	public static void Start(UILayout _Layout, float _Spacing)
	{
		if (_Layout == null)
			return;
		
		_Layout.EndLayout();
		
		Vector2 pivot = _Layout.RectTransform.pivot;
		
		float direction = pivot.x * 2 - 1;
		
		Rect rect = _Layout.GetLocalRect();
		
		HorizontalStackLayout layout = new HorizontalStackLayout(
			new Vector2(-rect.width * direction, 0),
			rect.height,
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

	public override Vector2 GetSize() => new Vector2(Mathf.Max(0, m_Width - Spacing), Height);

	public override Rect GetRect(Vector2 _Size)
	{
		float aspect = _Size.x / _Size.y;
		
		Vector2 container = new Vector2(Height, Height);
		
		Vector2 size = MathUtility.Fit(container, aspect);
		
		float direction = Pivot.x * 2 - 1;
		
		Rect rect = new Rect(
			Position.x + m_Width * direction + size.x * (1.0f - Pivot.x),
			Position.y,
			size.x,
			Height
		);
		
		m_Width += size.x + Spacing;
		
		return rect;
	}
}
