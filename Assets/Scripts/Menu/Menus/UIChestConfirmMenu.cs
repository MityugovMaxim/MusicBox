using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ChestConfirmMenu)]
public class UIChestConfirmMenu : UIDialog
{
	[SerializeField] UIChestImage m_Image;
	[SerializeField] UIUnitLabel  m_Coins;
	[SerializeField] Button       m_ConfirmButton;

	[Inject] ChestsManager         m_ChestsManager;
	[Inject] ProfileCoinsParameter m_ProfileCoins;

	RankType                   m_Rank;
	TaskCompletionSource<bool> m_Task;

	protected override void Awake()
	{
		base.Awake();
		
		m_ConfirmButton.Subscribe(Confirm);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ConfirmButton.Unsubscribe(Confirm);
	}

	public void Setup(RankType _Rank)
	{
		m_Rank        = _Rank;
		m_Image.Rank  = m_Rank;
		m_Coins.Value = m_ChestsManager.GetChestBoost(m_Rank);
	}

	public Task<bool> Process()
	{
		m_Task?.TrySetResult(false);
		
		m_Task = new TaskCompletionSource<bool>();
		
		return m_Task.Task;
	}

	protected override void OnHideFinished()
	{
		base.OnHideFinished();
		
		if (m_Task == null)
			return;
		
		m_Task.TrySetResult(false);
		m_Task = null;
	}

	async void Confirm()
	{
		long coins = m_ChestsManager.GetChestBoost(m_Rank);
		
		if (!await m_ProfileCoins.ReduceAsync(coins))
			return;
		
		if (m_Task == null)
			return;
		
		m_Task.TrySetResult(true);
		m_Task = null;
		
		Hide();
	}
}
