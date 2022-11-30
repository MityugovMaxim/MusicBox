using UnityEngine;
using UnityEngine.UI;

public class UIChestOpen : UIChestEntity
{
	static bool Processing { get; set; }

	[SerializeField] Button  m_OpenButton;
	[SerializeField] UIGroup m_OpenGroup;

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

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_OpenGroup.Hide(true);
	}

	protected override void Subscribe()
	{
		ChestsManager.SubscribeStart(ChestID, ProcessOpen);
		ChestsManager.SubscribeEnd(ChestID, ProcessOpen);
	}

	protected override void Unsubscribe()
	{
		ChestsManager.UnsubscribeStart(ChestID, ProcessOpen);
		ChestsManager.UnsubscribeEnd(ChestID, ProcessOpen);
	}

	protected override void ProcessData()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_OpenGroup.Show(true);
		else
			m_OpenGroup.Hide(true);
	}

	void ProcessOpen()
	{
		if (ChestsManager.IsStarted(ChestID))
			m_OpenGroup.Show();
		else
			m_OpenGroup.Hide();
	}

	async void Open()
	{
		if (Processing)
			return;
		
		Processing = true;
		
		await ChestsManager.Open(ChestID);
		
		Processing = false;
	}
}