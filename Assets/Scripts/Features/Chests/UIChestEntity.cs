using Zenject;

public abstract class UIChestEntity : UIEntity
{
	public string ChestID
	{
		get => m_ChestID;
		set
		{
			if (m_ChestID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_ChestID))
				Unsubscribe();
			
			m_ChestID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_ChestID))
				Subscribe();
		}
	}

	protected ChestsInventory ChestsInventory => m_ChestsInventory;
	protected ChestsManager   ChestsManager   => m_ChestsManager;

	[Inject] ChestsInventory m_ChestsInventory;
	[Inject] ChestsManager   m_ChestsManager;

	string m_ChestID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		ChestID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
