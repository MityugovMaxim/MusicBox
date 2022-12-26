using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UIChestsBadge : UIBadge
{
	[Inject] ChestsInventory m_ChestsInventory;

	protected override void Subscribe()
	{
		BadgeManager.SubscribeChests(Process);
		m_ChestsInventory.SubscribeEnd(Process);
		m_ChestsInventory.SubscribeStart(Process);
		m_ChestsInventory.SubscribeCancel(Process);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Add, Process);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Remove, Process);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Change, Process);
	}

	protected override void Unsubscribe()
	{
		BadgeManager.UnsubscribeChests(Process);
		m_ChestsInventory.UnsubscribeEnd(Process);
		m_ChestsInventory.UnsubscribeStart(Process);
		m_ChestsInventory.UnsubscribeCancel(Process);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Add, Process);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Change, Process);
	}

	protected override async void Preload()
	{
		await m_ChestsInventory.Activate();
		
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
		List<string> chestIDs = m_ChestsInventory.GetAvailableChestIDs();
		
		return chestIDs?.Count(_ChestID => BadgeManager.IsChestUnread(_ChestID)) ?? 0;
	}

	int GetReadyChestsCount()
	{
		List<string> chestIDs = m_ChestsInventory.GetSelectedChestIDs();
		
		return chestIDs?.Count(m_ChestsInventory.IsReady) ?? 0;
	}
}
