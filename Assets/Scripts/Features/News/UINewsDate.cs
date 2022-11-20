using TMPro;
using UnityEngine;
using Zenject;

public class UINewsDate : UIEntity
{
	public string NewsID
	{
		get => m_NewsID;
		set
		{
			if (m_NewsID == value)
				return;
			
			m_NewsManager.Collection.Unsubscribe(DataEventType.Change, m_NewsID, ProcessDate);
			
			m_NewsID = value;
			
			m_NewsManager.Collection.Subscribe(DataEventType.Change, m_NewsID, ProcessDate);
			
			ProcessDate();
		}
	}

	[SerializeField] TMP_Text m_Date;

	[Inject] NewsManager m_NewsManager;

	string m_NewsID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	public void Setup(string _NewsID)
	{
		NewsID = _NewsID;
	}

	void ProcessDate()
	{
		m_Date.text = m_NewsManager.GetDate(m_NewsID);
	}
}
