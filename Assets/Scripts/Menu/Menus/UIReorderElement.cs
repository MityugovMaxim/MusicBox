using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIReorderElement : UIEntity, IInitializePotentialDragHandler, IDragHandler, IDropHandler, IPointerExitHandler
{
	[Serializable]
	public class ReorderEvent : UnityEvent<int, int> { }

	[SerializeField] UILayout     m_Layout;
	[SerializeField] UIGroup      m_Source;
	[SerializeField] UIGroup      m_Target;
	[SerializeField] ReorderEvent m_Reorder;

	bool m_Pressed;
	int  m_SourceIndex;
	int  m_TargetIndex;
	Rect m_SourceRect;
	Rect m_TargetRect;

	void Update()
	{
		if (!m_Pressed)
			return;
		
		ProcessSource();
		ProcessTarget();
	}

	void ProcessSource()
	{
		RectTransform transform = m_Source.RectTransform;
		Vector2       pivot     = transform.pivot;
		transform.anchoredPosition = m_SourceRect.position - Vector2.Scale(m_SourceRect.size, pivot);
		transform.sizeDelta        = m_SourceRect.size;
	}

	void ProcessTarget()
	{
		RectTransform transform = m_Target.RectTransform;
		Vector2       pivot     = transform.pivot;
		transform.anchoredPosition = m_TargetRect.position - Vector2.Scale(m_TargetRect.size, pivot);
		transform.sizeDelta        = m_TargetRect.size;
	}

	void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData _EventData)
	{
		m_Pressed = true;
		
		Vector2 position = m_Layout.GetLocalPoint(_EventData.position, _EventData.pressEventCamera);
		
		(int index, Rect rect) = m_Layout.FindEntity(position);
		
		if (index < 0)
			return;
		
		rect = GetLocalRect(m_Layout.GetWorldRect(rect));
		
		m_Source.Show();
		m_Target.Show();
		
		m_SourceIndex = index;
		m_TargetIndex = index;
		m_SourceRect  = rect;
		m_TargetRect  = rect;
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		if (!m_Pressed)
			return;
		
		Vector2 position = m_Layout.GetLocalPoint(_EventData.position, _EventData.pressEventCamera);
		
		(int index, Rect rect) = m_Layout.FindEntity(position);
		
		if (index < 0)
			return;
		
		rect = GetLocalRect(m_Layout.GetWorldRect(rect));
		
		m_TargetIndex = index;
		m_TargetRect  = rect;
	}

	void IDropHandler.OnDrop(PointerEventData _EventData)
	{
		if (!m_Pressed)
			return;
		
		m_Pressed = false;
		
		m_Source.Hide();
		m_Target.Hide();
		
		m_Reorder?.Invoke(m_SourceIndex, m_TargetIndex);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		m_Pressed = false;
		
		m_Source.Hide();
		m_Target.Hide();
	}
}