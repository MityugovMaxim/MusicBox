using UnityEngine;
using Zenject;

public class UIChestAction : UIChestEntity
{
	public enum ActionType
	{
		Auto   = 0,
		Select = 1,
		Boost  = 2,
		Open   = 3,
	}

	[SerializeField] ActionType m_Type;
	[SerializeField] UIButton   m_Button;

	[Inject] Localization  m_Localization;
	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(ProcessAction);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(ProcessAction);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }

	void ProcessAction()
	{
		switch (m_Type)
		{
			case ActionType.Auto:
				if (ChestsInventory.IsAvailable(ChestID))
					ChestSelect();
				else if (ChestsInventory.IsProcessing(ChestID))
					ChestBoost();
				else if (ChestsInventory.IsReady(ChestID))
					ChestOpen();
				break;
			case ActionType.Select:
				if (ChestsInventory.IsAvailable(ChestID))
					ChestSelect();
				break;
			case ActionType.Boost:
				if (ChestsInventory.IsProcessing(ChestID))
					ChestBoost();
				break;
			case ActionType.Open:
				if (ChestsInventory.IsReady(ChestID))
					ChestOpen();
				break;
		}
	}

	async void ChestSelect()
	{
		await ChestsInventory.Select(ChestID);
	}

	async void ChestBoost()
	{
		RankType rank = ChestsInventory.GetRank(ChestID);
		
		long coins = ChestsManager.GetBoost(rank);
		
		bool confirm = await m_MenuProcessor.CoinsAsync(
			"chest_boost",
			m_Localization.Get("CHEST_BOOST_TITLE"),
			m_Localization.Get("CHEST_BOOST_MESSAGE"),
			coins
		);
		
		if (!confirm)
			return;
		
		ChestReward reward = await ChestsInventory.Boost(ChestID);
		
		if (reward == null)
			return;
		
		UIChestMenu chestMenu = m_MenuProcessor.GetMenu<UIChestMenu>();
		
		if (chestMenu == null)
			return;
		
		chestMenu.Setup(ChestID, reward);
		chestMenu.Show();
	}

	async void ChestOpen()
	{
		ChestReward reward = await ChestsInventory.Open(ChestID);
		
		if (reward == null)
			return;
		
		UIChestMenu chestMenu = m_MenuProcessor.GetMenu<UIChestMenu>();
		
		if (chestMenu == null)
			return;
		
		chestMenu.Setup(ChestID, reward);
		chestMenu.Show();
	}
}
