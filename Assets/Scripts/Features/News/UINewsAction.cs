using UnityEngine;
using UnityEngine.UI;
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

	[SerializeField] Button m_Button;

	[Inject] NewsManager   m_NewsManager;
	[Inject] UrlProcessor  m_UrlProcessor;
	[Inject] MenuProcessor m_MenuProcessor;

	string m_NewsID;
	string m_URL;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(Process);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(Process);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		NewsID = null;
	}

	void ProcessURL()
	{
		m_URL = m_NewsManager.GetURL(NewsID);
	}

	async void Process()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_UrlProcessor.ProcessURL(m_URL);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}
}
