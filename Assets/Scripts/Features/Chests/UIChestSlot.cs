using UnityEngine;
using Zenject;

public class UIChestSlot : UIOverlayButton
{
	[SerializeField] int          m_Slot;
	[SerializeField] UIChestBody  m_Body;
	[SerializeField] UIChestTimer m_Timer;
	[SerializeField] UIChestBoost m_Boost;
	[SerializeField] UIChestOpen  m_Open;

	[Inject] ChestsManager m_ChestsManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_ChestsManager.Profile.Subscribe(DataEventType.Add, ProcessChest);
		m_ChestsManager.Profile.Subscribe(DataEventType.Remove, ProcessChest);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_ChestsManager.Profile.Unsubscribe(DataEventType.Add, ProcessChest);
		m_ChestsManager.Profile.Unsubscribe(DataEventType.Remove, ProcessChest);
	}

	void ProcessChest(string _ChestID)
	{
		int slot = m_ChestsManager.GetSlot(_ChestID);
		
		if (m_Slot != slot)
			return;
		
		m_Body.ChestID  = _ChestID;
		m_Timer.ChestID = _ChestID;
		m_Boost.ChestID = _ChestID;
		m_Open.ChestID  = _ChestID;
	}

	protected override void OnClick()
	{
		base.OnClick();
		
		// TODO: Open chests inventory
	}
}
