using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIChestSlot : UIEntity
{
	[SerializeField] int           m_Slot;
	[SerializeField] UIChestIcon   m_Icon;
	[SerializeField] UIChestAction m_Action;
	[SerializeField] UIChestBoost  m_Boost;
	[SerializeField] UIChestTimer  m_Timer;
	[SerializeField] UIChestTime   m_Time;
	[SerializeField] UIHighlight   m_EmptyHighlight;
	[SerializeField] UIHighlight   m_SelectHighlight;
	[SerializeField] UIHighlight   m_ReadyHighlight;

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

	public async Task<bool> Select(RankType _ChestRank)
	{
		string chestID = m_ChestsInventory.GetAvailableChestID(_ChestRank);
		
		if (string.IsNullOrEmpty(chestID))
			return false;
		
		Unsubscribe();
		
		m_Icon.ChestRank = _ChestRank;
		
		m_Icon.Select();
		
		RequestState state = await m_ChestsInventory.Select(chestID);
		
		await m_ChestsInventory.Profile.UpdateAsync(chestID);
		
		ProcessData();
		
		Subscribe();
		
		return state == RequestState.Success;
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
		string chestID = m_ChestsInventory.GetChestID(m_Slot);
		
		RankType rank = m_ChestsInventory.GetRank(chestID);
		
		m_Icon.ChestRank = rank;
		m_Action.ChestID = chestID;
		m_Boost.ChestID  = chestID;
		m_Timer.ChestID  = chestID;
		m_Time.ChestID   = chestID;
		
		int slot = m_ChestsInventory.GetSlot();
		
		if (m_Slot == slot)
		{
			m_EmptyHighlight.Show();
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
		}
		else if (m_ChestsInventory.IsReady(chestID))
		{
			m_ReadyHighlight.Show();
			m_SelectHighlight.Hide();
			m_EmptyHighlight.Hide();
			m_Icon.Ready();
		}
		else if (m_ChestsInventory.IsProcessing(chestID))
		{
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
			m_EmptyHighlight.Hide();
			m_Icon.Process();
		}
		else
		{
			m_SelectHighlight.Hide();
			m_ReadyHighlight.Hide();
			m_EmptyHighlight.Hide();
		}
	}
}
