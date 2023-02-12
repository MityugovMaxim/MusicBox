using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UIChestsBadge : UIBadge
{
	[Inject] ChestsManager m_ChestsManager;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeChests(Process);
		m_ChestsManager.SubscribeChests(RankType.Bronze, Process);
		m_ChestsManager.SubscribeChests(RankType.Silver, Process);
		m_ChestsManager.SubscribeChests(RankType.Gold, Process);
		m_ChestsManager.SubscribeChests(RankType.Platinum, Process);
		m_ChestsManager.SubscribeStartTimer(Process);
		m_ChestsManager.SubscribeEndTimer(Process);
		m_ChestsManager.SubscribeCancelTimer(Process);
		m_ChestsManager.Slots.Subscribe(DataEventType.Add, Process);
		m_ChestsManager.Slots.Subscribe(DataEventType.Remove, Process);
		m_ChestsManager.Slots.Subscribe(DataEventType.Change, Process);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeChests(Process);
		m_ChestsManager.UnsubscribeChests(RankType.Bronze, Process);
		m_ChestsManager.UnsubscribeChests(RankType.Silver, Process);
		m_ChestsManager.UnsubscribeChests(RankType.Gold, Process);
		m_ChestsManager.UnsubscribeChests(RankType.Platinum, Process);
		m_ChestsManager.UnsubscribeStartTimer(Process);
		m_ChestsManager.UnsubscribeEndTimer(Process);
		m_ChestsManager.UnsubscribeCancelTimer(Process);
		m_ChestsManager.Slots.Unsubscribe(DataEventType.Add, Process);
		m_ChestsManager.Slots.Unsubscribe(DataEventType.Remove, Process);
		m_ChestsManager.Slots.Unsubscribe(DataEventType.Change, Process);
	}

	protected override async void Preload()
	{
		await m_ChestsManager.Activate();
		
		Process();
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetAvailableChestsCount();
		value += GetReadyChestsCount();
		
		SetValue(value);
	}

	int GetAvailableChestsCount()
	{
		int slots  = m_ChestsManager.GetSlotsCount(ChestSlotState.None);
		int chests = m_ChestsManager.GetChestCount();
		return Mathf.Min(chests, slots);
	}

	int GetReadyChestsCount()
	{
		return m_ChestsManager.GetSlotsCount(ChestSlotState.Ready);
	}
}
