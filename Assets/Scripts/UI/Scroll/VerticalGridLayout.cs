using UnityEngine;

public class VerticalStackLayout : Layout
{
	Vector2 Position { get; }
	float   Width    { get; }
	float   Spacing  { get; }

	float m_Height;

	public VerticalStackLayout(
		Vector2 _Position,
		float   _Width,
		float   _Spacing
	)
	{
		Position = _Position;
		Width    = _Width;
		Spacing  = _Spacing;
	}

	public static void Start(UILayout _Layout, float _Spacing)
	{
		if (_Layout == null)
			return;
		
		_Layout.EndLayout();
		
		Rect rect = _Layout.GetLocalRect();
		
		VerticalStackLayout layout = new VerticalStackLayout(
			new Vector2(0, -rect.height),
			rect.width,
			_Spacing
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
		Rect rect = new Rect(
			Position.x,
			Position.y - m_Height,
			Width,
			_Size.y
		);
		
		m_Height += _Size.y + Spacing;
		
		return rect;
	}
}

public class VerticalGridLayout : Layout
{
	Vector2 Position          { get; }
	int     ColumnCount       { get; }
	float   Width             { get; }
	float   Height            { get; }
	float   HorizontalSpacing { get; }
	float   VerticalSpacing   { get; }

	int m_Count;

	public VerticalGridLayout(
		Vector2 _Position,
		int     _ColumnCount,
		float   _Width,
		float   _Height,
		float   _HorizontalSpacing,
		float   _VerticalSpacing
	)
	{
		Position          = _Position;
		ColumnCount       = _ColumnCount;
		Width             = _Width;
		Height            = _Height;
		HorizontalSpacing = _HorizontalSpacing;
		VerticalSpacing   = _VerticalSpacing;
	}

	public static void Start(
		UILayout _Layout,
		int      _ColumnCount,
		float    _Aspect,
		float    _HorizontalSpacing,
		float    _VerticalSpacing
	)
	{
		if (_Layout == null)
			return;
		
		_Layout.EndLayout();
		
		Rect rect = _Layout.GetLocalRect();
		
		float spacing = Mathf.Max(0, _ColumnCount - 1) * _HorizontalSpacing;
		float width   = (rect.width - spacing) / _ColumnCount;
		float height  = width / _Aspect;
		
		Layout layout = new VerticalGridLayout(
			new Vector2(0, -rect.height),
			_ColumnCount,
			width,
			height,
			_HorizontalSpacing,
			_VerticalSpacing
		);
		
		_Layout.StartLayout(layout);
	}

	public static void End(UILayout _Layout)
	{
		if (_Layout != null)
			_Layout.EndLayout();
	}

	public override Vector2 GetSize()
	{
		int colCount = Mathf.Min(m_Count, ColumnCount);
		int rowCount = m_Count / ColumnCount;
		
		return new Vector2(
			colCount * Width + Mathf.Max(0, colCount - 1) * HorizontalSpacing,
			rowCount * Height + Mathf.Max(0, rowCount - 1) * VerticalSpacing
		);
	}

	public override Rect GetRect(Vector2 _Size)
	{
		int x = m_Count % ColumnCount;
		int y = m_Count / ColumnCount;
		
		Rect rect = new Rect(
			Position.x + x * Width + x * HorizontalSpacing,
			Position.y - y * Height - y * VerticalSpacing,
			Width,
			Height
		);
		
		m_Count++;
		
		return rect;
	}
}