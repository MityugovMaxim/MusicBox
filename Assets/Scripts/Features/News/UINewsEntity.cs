using Zenject;

public abstract class UINewsEntity : UIEntity
{
	public string NewsID
	{
		get => m_NewsID;
		set
		{
			if (m_NewsID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_NewsID))
				Unsubscribe();
			
			m_NewsID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_NewsID))
				Subscribe();
		}
	}

	protected NewsManager NewsManager => m_NewsManager;

	[Inject] NewsManager m_NewsManager;

	string m_NewsID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}
