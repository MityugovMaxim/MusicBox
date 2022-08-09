using UnityEngine.Scripting;
using Zenject;

public class UILatencyElement : UIOverlayButton
{
	[Preserve]
	public class Factory : PlaceholderFactory<string, UILatencyElement> { }

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void OnClick()
	{
		base.OnClick();
		
		Open();
	}

	async void Open()
	{
		await m_MenuProcessor.Show(MenuType.LatencyMenu);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		
		if (mainMenu == null)
			return;
		
		mainMenu.Select(MainMenuPageType.Profile);
	}
}