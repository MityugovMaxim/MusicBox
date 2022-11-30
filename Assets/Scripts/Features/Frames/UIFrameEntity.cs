using Zenject;

public abstract class UIFrameEntity : UIEntity
{
	public string FrameID
	{
		get => m_FrameID;
		set
		{
			if (m_FrameID == value)
				return;
			
			if (!string.IsNullOrEmpty(m_FrameID))
				Unsubscribe();
			
			m_FrameID = value;
			
			ProcessData();
			
			if (!string.IsNullOrEmpty(m_FrameID))
				Subscribe();
		}
	}

	protected FramesManager FramesManager => m_FramesManager;

	[Inject] FramesManager m_FramesManager;

	string m_FrameID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		FrameID = null;
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected abstract void ProcessData();
}