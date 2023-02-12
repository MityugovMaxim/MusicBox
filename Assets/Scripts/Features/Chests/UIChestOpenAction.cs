using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIChestOpenAction : UIEntity
{
	[SerializeField] int      m_Slot;
	[SerializeField] UIButton m_OpenButton;

	[Inject] ChestsManager         m_ChestsManager;
	[Inject] ProfileCoinsParameter m_ProfileCoins;
	[Inject] MenuProcessor         m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_OpenButton.Subscribe(Open);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_OpenButton.Unsubscribe(Open);
	}

	async void Open()
	{
		RankType rank = m_ChestsManager.GetSlotRank(m_Slot);
		
		if (rank == RankType.None)
			return;
		
		ChestSlotState state = m_ChestsManager.GetSlotState(m_Slot);
		
		if (state == ChestSlotState.None)
			return;
		
		bool confirm = await ConfirmAsync();
		
		if (!confirm)
			return;
		
		UIChestMenu chestMenu = m_MenuProcessor.GetMenu<UIChestMenu>();
		
		if (chestMenu == null)
			return;
		
		chestMenu.Setup(rank);
		
		await chestMenu.ShowAsync();
		
		ChestReward reward = await m_ChestsManager.OpenAsync(m_Slot);
		
		if (reward != null)
			chestMenu.Process(reward);
		else
			chestMenu.Hide(true);
		
		await m_ProfileCoins.Reload();
	}

	Task<bool> ConfirmAsync()
	{
		ChestSlotState state = m_ChestsManager.GetSlotState(m_Slot);
		
		if (state != ChestSlotState.Processing && state != ChestSlotState.Pending)
			return Task.FromResult(true);
		
		UIChestConfirmMenu chestConfirmMenu = m_MenuProcessor.GetMenu<UIChestConfirmMenu>();
		
		if (chestConfirmMenu == null)
			return Task.FromResult(true);
		
		RankType rank = m_ChestsManager.GetSlotRank(m_Slot);
		
		chestConfirmMenu.Setup(rank);
		chestConfirmMenu.Show();
		
		return chestConfirmMenu.Process();
	}
}
