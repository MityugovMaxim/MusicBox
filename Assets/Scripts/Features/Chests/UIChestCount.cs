using UnityEngine;
using Zenject;

public class UIChestCount : UIEntity
{
	[SerializeField] RankType    m_ChestRank;
	[SerializeField] UIUnitLabel m_Count;

	[Inject] ChestsInventory m_ChestsInventory;

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
		
		int count = m_ChestsInventory.GetChestsCount(m_ChestRank);
		
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
		m_ChestsInventory.Profile.Subscribe(DataEventType.Add, ProcessData);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Remove, ProcessData);
		m_ChestsInventory.Profile.Subscribe(DataEventType.Change, ProcessData);
	}

	void Unsubscribe()
	{
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		m_ChestsInventory.Profile.Unsubscribe(DataEventType.Change, ProcessData);
	}

	void ProcessData()
	{
		m_Count.Value = m_ChestsInventory.GetChestsCount(m_ChestRank);
	}
}
