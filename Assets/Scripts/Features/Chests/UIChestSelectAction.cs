using UnityEngine;
using Zenject;

public class UIChestSelectAction : UIEntity
{
	[SerializeField] RankType m_Rank;
	[SerializeField] UIButton m_SelectButton;

	[Inject] ChestsManager m_ChestsManager;
	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_SelectButton.Subscribe(Select);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_SelectButton.Unsubscribe(Select);
	}

	async void Select()
	{
		if (!m_ChestsManager.TryGetAvailableSlot(out int slot))
			return;
		
		int count = m_ChestsManager.GetChestCount(m_Rank);
		
		if (count <= 0)
			return;
		
		bool success = await m_ChestsManager.SelectAsync(m_Rank, slot);
		
		if (success)
			return;
		
		await m_MenuProcessor.RetryAsync("chest_select", Select);
	}
}
