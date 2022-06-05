using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQRCode : UIEntity
{
	public string Message
	{
		get => m_Message;
		set
		{
			if (m_Message == value)
				return;
			
			m_Message = value;
			
			ProcessMessage();
		}
	}

	[SerializeField] string        m_Message;
	[SerializeField] RectTransform m_AnchorBL;
	[SerializeField] RectTransform m_AnchorTL;
	[SerializeField] RectTransform m_AnchorTR;
	[SerializeField] RectTransform m_Content;
	[SerializeField] int           m_ContentSize = 11;
	[SerializeField] UIEntity      m_Dot;
	[SerializeField] RectTransform m_Container;

	readonly List<UIEntity> m_Dots = new List<UIEntity>();

	void ProcessMessage()
	{
		Clear();
		
		if (string.IsNullOrEmpty(Message))
			return;
		
		List<BitArray> matrix = QRCodeGenerator.Generate(m_Message, QRCodeGenerator.ErrorCorrection.H);
		
		if (matrix == null || matrix.Count == 0)
			return;
		
		Rect rect = m_Container.rect;
		
		float step = Mathf.Min(
			rect.width / matrix.Count,
			rect.height / matrix.Count
		);
		
		Vector2 size = new Vector2(step, step);
		
		Vector2 total = new Vector2(step * matrix.Count, step * matrix.Count);
		
		Vector2 pivot = m_Container.pivot;
		
		Vector2 position = rect.position + Vector2.Scale(rect.size - total, pivot) + size * 0.5f;
		
		int contentSize = m_ContentSize / 2 * 2 + 1;
		
		if (m_Content != null)
			m_Content.sizeDelta = size * contentSize - new Vector2(2, 2);
		
		m_AnchorBL.sizeDelta = size * 7;
		m_AnchorTL.sizeDelta = size * 7;
		m_AnchorTR.sizeDelta = size * 7;
		
		RectInt contentRect = new RectInt(
			(matrix.Count - contentSize) / 2,
			(matrix.Count - contentSize) / 2,
			contentSize,
			contentSize
		);
		
		RectInt anchorBL = new RectInt(0, 0, 7, 7);
		RectInt anchorTL = new RectInt(0, matrix.Count - 7, 7, 7);
		RectInt anchorTR = new RectInt(matrix.Count - 7, matrix.Count - 7, 7, 7);
		
		for (int y = 0; y < matrix.Count; y++)
		for (int x = 0; x < matrix[y].Length; x++)
		{
			if (matrix[x][matrix.Count - y - 1] == false)
				continue;
			
			Vector2Int point = new Vector2Int(x, y);
			
			if (m_Content != null && contentRect.Contains(point))
				continue;
			
			if (anchorBL.Contains(point))
				continue;
			
			if (anchorTL.Contains(point))
				continue;
			
			if (anchorTR.Contains(point))
				continue;
			
			UIEntity dot = Instantiate(m_Dot, m_Container, false);
			
			dot.RectTransform.anchoredPosition = position + new Vector2(size.x * x, size.y * y);
			dot.RectTransform.sizeDelta        = size;
			
			m_Dots.Add(dot);
		}
	}

	void Clear()
	{
		foreach (UIEntity dot in m_Dots)
		{
			if (dot != null)
				DestroyImmediate(dot.gameObject);
		}
		while (m_Container.childCount > 0)
			DestroyImmediate(m_Container.GetChild(0).gameObject);
		m_Dots.Clear();
	}
}