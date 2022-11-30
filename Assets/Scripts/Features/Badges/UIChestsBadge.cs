using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UIChestsBadge : UIBadge
{
	public const string CHESTS_GROUP = "chests";

	[Inject] ChestsManager m_ChestsManager;

	List<string> m_ChestIDs;

	protected override void Subscribe()
	{
		m_ChestIDs = m_ChestsManager.GetChestIDs();
		
		m_ChestsManager.Profile.Subscribe(DataEventType.Add, Process);
		m_ChestsManager.Profile.Subscribe(DataEventType.Remove, Process);
		m_ChestsManager.Profile.Subscribe(DataEventType.Change, Process);
		
		if (m_ChestIDs == null)
			return;
		
		foreach (string chestID in m_ChestIDs)
		{
			m_ChestsManager.SubscribeStart(chestID, Process);
			m_ChestsManager.SubscribeEnd(chestID, Process);
		}
	}

	protected override void Unsubscribe()
	{
		m_ChestsManager.Profile.Unsubscribe(DataEventType.Add, Process);
		m_ChestsManager.Profile.Unsubscribe(DataEventType.Remove, Process);
		m_ChestsManager.Profile.Unsubscribe(DataEventType.Change, Process);
		
		if (m_ChestIDs == null)
			return;
		
		foreach (string chestID in m_ChestIDs)
			m_ChestsManager.UnsubscribeEnd(chestID, Process);
	}

	protected override void Process()
	{
		int value = 0;
		
		value += GetAvailableChestsCount();
		value += GetSelectedChestsCount();
		
		SetValue(value);
	}

	int GetAvailableChestsCount()
	{
		List<string> chestIDs = m_ChestsManager.GetAvailableChestIDs();
		
		return chestIDs?.Count(_ChestID => BadgeManager.IsUnread(CHESTS_GROUP, _ChestID)) ?? 0;
	}

	int GetSelectedChestsCount()
	{
		List<string> chestIDs = m_ChestsManager.GetSelectedChestIDs();
		
		return chestIDs?.Count(_ChestID => m_ChestsManager.IsEnded(_ChestID)) ?? 0;
	}
}
