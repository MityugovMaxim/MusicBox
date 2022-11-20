using UnityEngine;
using Zenject;

public class UINewsImage : UIEntity
{
	public string NewsID
	{
		get => m_NewsID;
		set
		{
			if (m_NewsID == value)
				return;
			
			m_NewsManager.Collection.Unsubscribe(DataEventType.Change, m_NewsID, ProcessImage);
			
			m_NewsID = value;
			
			ProcessImage();
			
			m_NewsManager.Collection.Subscribe(DataEventType.Change, m_NewsID, ProcessImage);
		}
	}

	[SerializeField] WebImage m_Image;

	[Inject] NewsManager m_NewsManager;

	string m_NewsID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	void ProcessImage()
	{
		m_Image.Path = m_NewsManager.GetImage(NewsID);
	}
}
