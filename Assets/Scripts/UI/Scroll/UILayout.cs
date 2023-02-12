using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction
{
	Up    = 0,
	Down  = 1,
	Left  = 2,
	Right = 3
}

public class UILayout : UIEntity
{
	bool IsHorizontal => m_Direction == Direction.Left || m_Direction == Direction.Right;
	bool IsVertical => m_Direction == Direction.Down || m_Direction == Direction.Up;

	[SerializeField] Direction  m_Direction = Direction.Down;
	[SerializeField] UIEntity   m_Viewport;
	[SerializeField] RectOffset m_Margin;

	readonly List<LayoutEntity>       m_Items = new List<LayoutEntity>();
	readonly Dictionary<string, Rect> m_Rects = new Dictionary<string, Rect>();

	int MinIndex { get; set; } = -1;
	int MaxIndex { get; set; } = -1;

	Layout m_Layout;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		Reposition();
	}

	public void StartLayout(Layout _Layout)
	{
		m_Layout = _Layout;
	}

	public void EndLayout()
	{
		if (m_Layout == null)
			return;
		
		Vector2 size = RectTransform.sizeDelta;
		
		if (IsVertical)
			size.y += m_Layout.GetHeight();
		
		if (IsHorizontal)
			size.x += m_Layout.GetWidth();
		
		RectTransform.sizeDelta = size;
		
		m_Layout = null;
	}

	public void Spacing(float _Value)
	{
		if (m_Items.Count > 0)
			Space(_Value);
	}

	public void Space(float _Value)
	{
		Vector2 size = RectTransform.sizeDelta;
		
		if (IsVertical)
			size.y += _Value;
		
		if (IsHorizontal)
			size.x += _Value;
		
		RectTransform.sizeDelta = size;
	}

	public void Add(LayoutEntity _Item)
	{
		m_Items.Add(_Item);
		
		_Item.Rect = m_Layout.GetRect(_Item.Size);
		
		m_Rects[_Item.ID] = _Item.Rect;
	}

	public void Remove(LayoutEntity _Item)
	{
		m_Items.Remove(_Item);
		m_Rects.Remove(_Item.ID);
	}

	public void Refresh(string _ID)
	{
		LayoutEntity item = m_Items.FirstOrDefault(_Item => _Item != null && _Item.ID == _ID);
		
		item?.Refresh();
	}

	public bool Contains(string _ID) => m_Rects.ContainsKey(_ID);

	public void Clear()
	{
		EndLayout();
		
		MinIndex = -1;
		MaxIndex = -1;
		
		Vector2 size = RectTransform.sizeDelta;
		
		if (IsVertical)
			size.y = 0;
		
		if (IsHorizontal)
			size.x = 0;
		
		RectTransform.sizeDelta = size;
		
		foreach (LayoutEntity item in m_Items)
			item.Remove();
		m_Items.Clear();
		m_Rects.Clear();
	}

	public Vector2 GetPosition(string _ID, TextAnchor _Alignment)
	{
		m_Rects.TryGetValue(_ID, out Rect rect);
		
		Vector2 pivot = _Alignment.GetPivot();
		
		return rect.position + Vector2.Scale(rect.size, -pivot);
	}

	public void Reposition()
	{
		EndLayout();
		
		Rect rect = GetLocalRect(m_Viewport.GetWorldRect());
		
		float min = 0;
		float max = 0;
		
		if (IsVertical)
		{
			min = rect.yMin - m_Margin.bottom;
			max = rect.yMax + m_Margin.top;
		}
		
		if (IsHorizontal)
		{
			min = rect.xMin + m_Margin.left;
			max = rect.xMax - m_Margin.right;
		}
		
		(int minIndex, int maxIndex) = GetRange(min, max);
		
		// Remove items
		for (int i = Mathf.Max(0, MinIndex); i <= MaxIndex; i++)
		{
			LayoutEntity item = m_Items[i];
			
			if (item == null || i >= minIndex && i <= maxIndex)
				continue;
			
			item.Remove();
		}
		
		// Create items
		for (int i = Mathf.Max(0, minIndex); i <= maxIndex; i++)
		{
			LayoutEntity item = m_Items[i];
			
			if (item == null || i >= MinIndex && i <= MaxIndex)
				continue;
			
			item.Create(RectTransform);
		}
		
		MinIndex = minIndex;
		MaxIndex = maxIndex;
	}

	public (int index, Rect rect) FindEntity(Vector2 _Position)
	{
		if (m_Items == null || m_Items.Count == 0)
			return (-1, Rect.zero);
		
		float position = 0;
		
		if (IsVertical)
			position = _Position.y;
		
		if (IsHorizontal)
			position = _Position.x;
		
		(int minIndex, int maxIndex) = GetRange(position, position);
		
		int index = (minIndex + maxIndex) / 2;
		
		if (index < 0)
			return (-1, Rect.zero);
		
		return (index, m_Items[index].Rect);
	}

	(int minIndex, int maxIndex) GetRange(float _Min, float _Max)
	{
		int anchor = FindAnchor(_Min, _Max);
		
		if (anchor < 0)
			return (anchor, anchor);
		
		int minIndex = FindMin(anchor, _Max);
		int maxIndex = FindMax(anchor, _Min);
		
		return (minIndex, maxIndex);
	}

	int FindMin(int _Anchor, float _Max)
	{
		bool vertical   = IsVertical;
		bool horizontal = IsHorizontal;
		
		int index = _Anchor;
		while (index > 0)
		{
			LayoutEntity item = m_Items[index - 1];
			
			Rect rect = item.Rect;
			
			float min = 0;
			
			if (vertical)
				min = rect.y - rect.height;
			
			if (horizontal)
				min = rect.x;
			
			if (min > _Max)
				break;
			
			index--;
		}
		return index;
	}

	int FindMax(int _Anchor, float _Min)
	{
		bool vertical   = IsVertical;
		bool horizontal = IsHorizontal;
		
		int index = _Anchor;
		while (index < m_Items.Count - 1)
		{
			LayoutEntity item = m_Items[index + 1];
			
			Rect rect = item.Rect;
			
			float max = 0;
			
			if (vertical)
				max = rect.y;
			
			if (horizontal)
				max = rect.x + rect.width;
			
			if (max < _Min)
				break;
			
			index++;
		}
		return index;
	}

	int FindAnchor(float _Min, float _Max)
	{
		bool vertical   = IsVertical;
		bool horizontal = IsHorizontal;
		
		int i = 0;
		int j = m_Items.Count - 1;
		while (i <= j)
		{
			int k = (i + j) / 2;
			
			LayoutEntity item = m_Items[k];
			
			Rect rect = item.Rect;
			
			float min = 0;
			float max = 0;
			
			if (vertical)
			{
				min = rect.y - rect.height;
				max = rect.y;
			}
			
			if (horizontal)
			{
				min = rect.x;
				max = rect.x + rect.width;
			}
			
			if (min > _Max)
				i = k + 1;
			else if (max < _Min)
				j = k - 1;
			else
				return k;
		}
		return -1;
	}
}
