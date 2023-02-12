using UnityEngine;
using Zenject;

public abstract class UISlotEntity : UIEntity
{
	public int Slot
	{
		get => m_Slot;
		set => m_Slot = value;
	}

	protected ChestsManager ChestsManager => m_ChestsManager;

	[SerializeField] int m_Slot;

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

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
