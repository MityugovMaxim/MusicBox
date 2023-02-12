using System.Threading.Tasks;
using UnityEngine;

public class UIChestSlot : UISlotEntity
{
	[SerializeField] UIChestImage   m_Image;
	[SerializeField] UIHighlight    m_EmptyHighlight;
	[SerializeField] UIHighlight    m_SelectHighlight;
	[SerializeField] UIHighlight    m_ReadyHighlight;
	[SerializeField] UISlotEntity[] m_Entities;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying || !IsInstanced)
			return;
		
		foreach (UISlotEntity entity in m_Entities)
			entity.Slot = Slot;
	}
	#endif

	public void Highlight(bool _Value)
	{
		if (_Value)
		{
			m_SelectHighlight.Show();
			m_EmptyHighlight.Hide();
		}
		else
		{
			ProcessData();
		}
	}

	public async Task<bool> Select(RankType _Rank)
	{
		if (!ChestsManager.TryGetAvailableSlot(out int slot))
			return false;
		
		Unsubscribe();
		
		m_Image.Rank = _Rank;
		
		m_Image.Select();
		
		bool success = await ChestsManager.SelectAsync(_Rank, slot);
		
		await ChestsManager.UpdateChestsAsync(_Rank);
		
		ProcessData();
		
		Subscribe();
		
		return success;
	}

	protected override void Subscribe()
	{
		ChestsManager.SubscribeStartTimer(Slot, ProcessData);
		ChestsManager.SubscribeEndTimer(Slot, ProcessData);
		ChestsManager.SubscribeCancelTimer(Slot, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Add, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Remove, ProcessData);
		ChestsManager.Slots.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.UnsubscribeStartTimer(Slot, ProcessData);
		ChestsManager.UnsubscribeEndTimer(Slot, ProcessData);
		ChestsManager.UnsubscribeCancelTimer(Slot, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Add, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Remove, ProcessData);
		ChestsManager.Slots.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		RankType rank = ChestsManager.GetSlotRank(Slot);
		
		m_Image.Rank = rank;
		
		ChestSlotState state = ChestsManager.GetSlotState(Slot);
		
		if (ChestsManager.TryGetAvailableSlot(out int slot) && Slot == slot)
		{
			m_EmptyHighlight.Show();
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
			m_Image.Restore();
		}
		else if (state == ChestSlotState.Ready)
		{
			m_ReadyHighlight.Show();
			m_SelectHighlight.Hide();
			m_EmptyHighlight.Hide();
			m_Image.Ready();
		}
		else if (state == ChestSlotState.Processing || state == ChestSlotState.Pending)
		{
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
			m_EmptyHighlight.Hide();
			m_Image.Process();
		}
		else
		{
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
			m_EmptyHighlight.Hide();
			m_Image.Restore();
		}
	}
}
