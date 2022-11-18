using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public class UINewsItem : UIEntity
{
	[Preserve]
	public class Pool : UIEntityPool<UINewsItem> { }

	[SerializeField] UINewsImage  m_Image;
	[SerializeField] UINewsLabel  m_Label;
	[SerializeField] UINewsDate   m_Date;
	[SerializeField] UINewsAction m_Action;
	[SerializeField] Button       m_OpenButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_OpenButton.Subscribe(Open);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_OpenButton.Unsubscribe(Open);
	}

	public void Setup(string _NewsID)
	{
		m_Image.NewsID  = _NewsID;
		m_Label.NewsID  = _NewsID;
		m_Date.NewsID   = _NewsID;
		m_Action.NewsID = _NewsID;
	}

	async void Open()
	{
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_Action.Process();
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}
}
