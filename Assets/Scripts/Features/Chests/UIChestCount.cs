using UnityEngine;
using Zenject;

public class UIChestCount : UIEntity
{
	[SerializeField] RankType    m_ChestRank;
	[SerializeField] UIUnitLabel m_Count;

	[Inject] ChestsManager m_ChestsManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessData();
		
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Unsubscribe();
	}

	public void Reduce()
	{
		Unsubscribe();
		
		int count = m_ChestsManager.GetChestCount(m_ChestRank);
		
		m_Count.Value = Mathf.Max(0, count - 1);
	}

	public void Restore()
	{
		ProcessData();
		
		Unsubscribe();
		
		Subscribe();
	}

	void Subscribe()
	{
		m_ChestsManager.SubscribeChests(m_ChestRank, ProcessData);
	}

	void Unsubscribe()
	{
		m_ChestsManager.UnsubscribeChests(m_ChestRank, ProcessData);
	}

	void ProcessData()
	{
		m_Count.Value = m_ChestsManager.GetChestCount(m_ChestRank);
	}
}
