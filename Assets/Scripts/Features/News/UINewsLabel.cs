using TMPro;
using UnityEngine;
using Zenject;

public class UINewsLabel : UIEntity
{
	public string NewsID
	{
		get => m_NewsID;
		set
		{
			if (m_NewsID == value)
				return;
			
			m_NewsManager.Descriptor.Unsubscribe(DataEventType.Add, m_NewsID, ProcessLabel);
			m_NewsManager.Descriptor.Unsubscribe(DataEventType.Remove, m_NewsID, ProcessLabel);
			m_NewsManager.Descriptor.Unsubscribe(DataEventType.Change, m_NewsID, ProcessLabel);
			m_NewsManager.Collection.Unsubscribe(DataEventType.Change, m_NewsID, ProcessLabel);
			
			m_NewsID = value;
			
			ProcessLabel();
			
			m_NewsManager.Descriptor.Subscribe(DataEventType.Add, m_NewsID, ProcessLabel);
			m_NewsManager.Descriptor.Subscribe(DataEventType.Remove, m_NewsID, ProcessLabel);
			m_NewsManager.Descriptor.Subscribe(DataEventType.Change, m_NewsID, ProcessLabel);
			m_NewsManager.Collection.Subscribe(DataEventType.Change, m_NewsID, ProcessLabel);
		}
	}

	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	[Inject] NewsManager m_NewsManager;

	string m_NewsID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	void ProcessLabel()
	{
		m_Title.text       = m_NewsManager.GetTitle(m_NewsID);
		m_Description.text = m_NewsManager.GetDescription(m_NewsID);
	}
}
