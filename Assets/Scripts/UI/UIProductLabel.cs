using TMPro;
using UnityEngine;
using Zenject;

public class UIProductLabel : UIEntity
{
	public string ProductID
	{
		get => m_ProductID;
		set
		{
			if (m_ProductID == value)
				return;
			
			m_ProductsManager.Descriptor.Unsubscribe(DataEventType.Add, m_ProductID, ProcessLabel);
			m_ProductsManager.Descriptor.Unsubscribe(DataEventType.Remove, m_ProductID, ProcessLabel);
			m_ProductsManager.Descriptor.Unsubscribe(DataEventType.Change, m_ProductID, ProcessLabel);
			
			m_ProductID = value;
			
			m_ProductsManager.Descriptor.Subscribe(DataEventType.Add, m_ProductID, ProcessLabel);
			m_ProductsManager.Descriptor.Subscribe(DataEventType.Remove, m_ProductID, ProcessLabel);
			m_ProductsManager.Descriptor.Subscribe(DataEventType.Change, m_ProductID, ProcessLabel);
			
			ProcessLabel();
		}
	}

	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;

	[Inject] ProductsManager m_ProductsManager;

	string m_ProductID;

	protected override void OnDisable()
	{
		base.OnDisable();
		
		ProductID = null;
	}

	void ProcessLabel()
	{
		m_Title.text       = m_ProductsManager.GetTitle(ProductID);
		m_Description.text = m_ProductsManager.GetDescription(ProductID);
	}
}
