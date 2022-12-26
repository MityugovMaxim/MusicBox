using UnityEngine;

public class HorizontalGridLayout : Layout
{
	Vector2 Position          { get; }
	int     RowCount          { get; }
	float   Width             { get; }
	float   Height            { get; }
	float   HorizontalSpacing { get; }
	float   VerticalSpacing   { get; }
	Vector2 Pivot             { get; }

	int m_Count;

	public HorizontalGridLayout(
		Vector2 _Position,
		int     _RowCount,
		float   _Width,
		float   _Height,
		float   _HorizontalSpacing,
		float   _VerticalSpacing,
		Vector2 _Pivot
	)
	{
		Position          = _Position;
		RowCount       = _RowCount;
		Width             = _Width;
		Height            = _Height;
		HorizontalSpacing = _HorizontalSpacing;
		VerticalSpacing   = _VerticalSpacing;
		Pivot             = _Pivot;
	}

	public static void Start(
		UILayout _Layout,
		int      _RowCount,
		float    _Aspect,
		float    _HorizontalSpacing,
		float    _VerticalSpacing
	)
	{
		if (_Layout == null)
			return;
		
		_Layout.EndLayout();
		
		Vector2 pivot = _Layout.RectTransform.pivot;
		
		float direction = pivot.x * 2 - 1;
		
		Rect rect = _Layout.GetLocalRect();
		
		float spacing = Mathf.Max(0, _RowCount - 1) * _VerticalSpacing;
		float height  = (rect.height - spacing) / _RowCount;
		float width   = height * _Aspect;
		
		Layout layout = new HorizontalGridLayout(
			new Vector2(-rect.width * direction, 0),
			_RowCount,
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
		int colCount = Mathf.CeilToInt((float)m_Count / RowCount);
		int rowCount = Mathf.Min(m_Count, RowCount);
		
		return new Vector2(
			colCount * Width + Mathf.Max(0, colCount - 1) * HorizontalSpacing,
			rowCount * Height + Mathf.Max(0, rowCount - 1) * VerticalSpacing
		);
	}

	public override Rect GetRect(Vector2 _Size)
	{
		float direction = Pivot.x * 2 - 1;
		
		int x = m_Count / RowCount;
		int y = m_Count % RowCount;
		
		Rect rect = new Rect(
			Position.x - x * Width * direction + x * HorizontalSpacing,
			Position.y + y * Height * direction - y * VerticalSpacing,
			Width,
			Height
		);
		
		m_Count++;
		
		return rect;
	}
}
