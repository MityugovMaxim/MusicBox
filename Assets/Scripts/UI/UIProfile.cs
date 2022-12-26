using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIProfile : UIEntity
{
	[SerializeField] Button m_OpenButton;

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

	void Open()
	{
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		if (mainMenu == null)
			return;
		
		mainMenu.Select(MainMenuPageType.Chests);
	}
}
