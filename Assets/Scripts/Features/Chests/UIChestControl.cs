using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIChestControl : UIEntity, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	static bool m_Drag;

	[SerializeField] RankType      m_ChestRank;
	[SerializeField] UIChestSlot[] m_Slots;
	[SerializeField] UIGroup       m_Content;
	[SerializeField] UIEntity      m_Pointer;
	[SerializeField] UIChestCount  m_Count;

	[Inject] ChestsManager m_ChestsManager;

	Vector2 m_Offset;

	void IBeginDragHandler.OnBeginDrag(PointerEventData _EventData)
	{
		if (m_Drag)
			return;
		
		int count = m_ChestsManager.GetChestCount(m_ChestRank);
		
		if (count <= 0)
			return;
		
		m_Drag = true;
		
		BringToFront();
		
		m_Content.Show();
		
		m_Pointer.RectTransform.anchoredPosition = Vector2.zero;
		
		Vector2 position = m_Content.GetLocalPoint(_EventData.position);
		
		m_Offset = m_Pointer.GetLocalPoint(_EventData.position);
		
		m_Pointer.RectTransform.anchoredPosition = position - m_Offset;
		
		m_Count.Reduce();
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		if (!m_Drag)
			return;
		
		Vector2 position = m_Content.GetLocalPoint(_EventData.position);
		
		m_Pointer.RectTransform.anchoredPosition = position - m_Offset;
	}

	async void IEndDragHandler.OnEndDrag(PointerEventData _EventData)
	{
		if (!m_Drag)
			return;
		
		Highlight(false);
		
		m_Content.Hide();
		
		await SelectAsync();
		
		m_Drag = false;
		
		Vector2 position = m_Content.GetLocalPoint(_EventData.position);
		
		m_Pointer.RectTransform.anchoredPosition = position - m_Offset;
		
		m_Count.Restore();
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		if (m_Drag)
			return;
		
		int count = m_ChestsManager.GetChestCount(m_ChestRank);
		
		if (count <= 0)
			return;
		
		m_Count.Reduce();
		
		m_Content.Hide();
		
		await SelectAsync();
		
		Vector2 position = m_Content.GetLocalPoint(_EventData.position);
		
		m_Pointer.RectTransform.anchoredPosition = position - m_Offset;
		
		m_Count.Restore();
	}

	void Highlight(bool _Value)
	{
		if (_Value)
		{
			UIChestSlot slot = GetSlot();
			if (slot != null)
				slot.Highlight(true);
		}
		else
		{
			foreach (UIChestSlot slot in m_Slots)
			{
				if (slot != null)
					slot.Highlight(false);
			}
		}
	}

	Task<bool> SelectAsync()
	{
		UIChestSlot slot = GetSlot();
		
		if (slot == null)
			return Task.FromResult(false);
		
		Rect source = slot.GetWorldRect();
		Rect target = m_Pointer.GetWorldRect();
		
		if (!m_Drag || source.Overlaps(target, true))
			return slot.Select(m_ChestRank);
		
		return Task.FromResult(false);
	}

	UIChestSlot GetSlot()
	{
		if (!m_ChestsManager.TryGetAvailableSlot(out int slot))
			return null;
		
		return slot >= 0 && slot < m_Slots.Length ? m_Slots[slot] : null;
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		Highlight(true);
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		Highlight(false);
	}
}
