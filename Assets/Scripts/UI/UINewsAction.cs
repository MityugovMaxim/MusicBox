using System.Threading.Tasks;
using Zenject;

public class UINewsAction : UIEntity
{
	public string NewsID
	{
		get => m_NewsID;
		set
		{
			if (m_NewsID == value)
				return;
			
			m_NewsManager.Collection.Unsubscribe(DataEventType.Change, m_NewsID, ProcessURL);
			
			m_NewsID = value;
			
			m_NewsManager.Collection.Subscribe(DataEventType.Change, m_NewsID, ProcessURL);
			
			ProcessURL();
		}
	}

	[Inject] NewsManager  m_NewsManager;
	[Inject] UrlProcessor m_UrlProcessor;

	string m_NewsID;
	string m_URL;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	void ProcessURL()
	{
		m_URL = m_NewsManager.GetURL(NewsID);
	}

	public Task Process() => m_UrlProcessor.ProcessURL(m_URL);
}