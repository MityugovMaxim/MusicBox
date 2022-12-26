using UnityEngine;

public class VerticalGridLayout : Layout
{
	Vector2 Position          { get; }
	int     ColumnCount       { get; }
	float   Width             { get; }
	float   Height            { get; }
	float   HorizontalSpacing { get; }
	float   VerticalSpacing   { get; }
	Vector2 Pivot             { get; }

	int m_Count;

	public VerticalGridLayout(
		Vector2 _Position,
		int     _ColumnCount,
		float   _Width,
		float   _Height,
		float   _HorizontalSpacing,
		float   _VerticalSpacing,
		Vector2 _Pivot
	)
	{
		Position          = _Position;
		ColumnCount       = _ColumnCount;
		Width             = _Width;
		Height            = _Height;
		HorizontalSpacing = _HorizontalSpacing;
		VerticalSpacing   = _VerticalSpacing;
		Pivot             = _Pivot;
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
		
		Vector2 pivot = _Layout.RectTransform.pivot;
		
		float direction = pivot.y * 2 - 1;
		
		Rect rect = _Layout.GetLocalRect();
		
		float spacing = Mathf.Max(0, _ColumnCount - 1) * _HorizontalSpacing;
		float width   = (rect.width - spacing) / _ColumnCount;
		float height  = width / _Aspect;
		
		Layout layout = new VerticalGridLayout(
			new Vector2(0, -rect.height * direction),
			_ColumnCount,
			width,
			height,
			_HorizontalSpacing,
			_VerticalSpacing,
			pivot
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
		int rowCount = Mathf.CeilToInt((float)m_Count / ColumnCount);
		
		return new Vector2(
			colCount * Width + Mathf.Max(0, colCount - 1) * HorizontalSpacing,
			rowCount * Height + Mathf.Max(0, rowCount - 1) * VerticalSpacing
		);
	}

	public override Rect GetRect(Vector2 _Size)
	{
		float direction = Pivot.y * 2 - 1;
		
		int x = m_Count % ColumnCount;
		int y = m_Count / ColumnCount;
		
		Rect rect = new Rect(
			Position.x + x * Width + x * HorizontalSpacing,
			Position.y - y * Height * direction - y * VerticalSpacing,
			Width,
			Height
		);
		
		m_Count++;
		
		return rect;
	}
}
