using System.Collections.Generic;
using UnityEngine;

public class UILayout : UIEntity
{
	[SerializeField] UIEntity m_Viewport;

	readonly List<LayoutEntity> m_Items = new List<LayoutEntity>();

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
		
		size.y += m_Layout.GetHeight();
		
		RectTransform.sizeDelta = size;
		
		m_Layout = null;
	}

	public void Space(float _Value)
	{
		Vector2 size = RectTransform.sizeDelta;
		
		size.y += _Value;
		
		RectTransform.sizeDelta = size;
	}

	public void Add(LayoutEntity _Item)
	{
		m_Items.Add(_Item);
		
		_Item.Rect = m_Layout.GetRect(_Item.Size);
	}

	public void Remove(LayoutEntity _Item)
	{
		m_Items.Remove(_Item);
	}

	public void Clear()
	{
		EndLayout();
		
		MinIndex = -1;
		MaxIndex = -1;
		
		Vector2 size = RectTransform.sizeDelta;
		
		size.y = 0;
		
		RectTransform.sizeDelta = size;
		
		foreach (LayoutEntity item in m_Items)
			item.Remove();
		m_Items.Clear();
	}

	public void Reposition()
	{
		EndLayout();
		
		Rect rect = GetLocalRect(m_Viewport.GetWorldRect());
		
		float min = rect.yMin;
		float max = rect.yMax;
		
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
		
		(int minIndex, int maxIndex) = GetRange(_Position.y, _Position.y);
		
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
		int index = _Anchor;
		while (index > 0)
		{
			LayoutEntity item = m_Items[index - 1];
			
			Rect rect = item.Rect;
			
			float min = rect.y - rect.height;
			
			if (min > _Max)
				break;
			
			index--;
		}
		return index;
	}

	int FindMax(int _Anchor, float _Min)
	{
		int index = _Anchor;
		while (index < m_Items.Count - 1)
		{
			LayoutEntity item = m_Items[index + 1];
			
			Rect rect = item.Rect;
			
			float max = rect.y;
			
			if (max < _Min)
				break;
			
			index++;
		}
		return index;
	}

	int FindAnchor(float _Min, float _Max)
	{
		int i = 0;
		int j = m_Items.Count - 1;
		while (i <= j)
		{
			int k = (i + j) / 2;
			
			LayoutEntity item = m_Items[k];
			
			Rect rect = item.Rect;
			
			float min = rect.y - rect.height;
			float max = rect.y;
			
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
