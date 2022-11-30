using UnityEngine;
using UnityEngine.UI;

public class UIChestBoost : UIChestEntity
{
	static bool Processing { get; set; }

	[SerializeField] Button  m_BoostButton;
	[SerializeField] UIGroup m_BoostGroup;

	protected override void Awake()
	{
		base.Awake();
		
		m_BoostButton.Subscribe(Boost);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BoostButton.Unsubscribe(Boost);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_BoostGroup.Hide(true);
	}

	protected override void Subscribe()
	{
		ChestsManager.SubscribeStart(ChestID, ProcessBoost);
		ChestsManager.SubscribeEnd(ChestID, ProcessBoost);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.UnsubscribeStart(ChestID, ProcessBoost);
		ChestsManager.UnsubscribeEnd(ChestID, ProcessBoost);
	}

	protected override void ProcessData()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_BoostGroup.Show(true);
		else
			m_BoostGroup.Hide(true);
	}

	void ProcessBoost()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_BoostGroup.Show();
		else
			m_BoostGroup.Hide();
	}

	async void Boost()
	{
		if (Processing)
			return;
		
		Processing = true;
		
		await ChestsManager.Boost(ChestID);
		
		Processing = false;
	}
}
