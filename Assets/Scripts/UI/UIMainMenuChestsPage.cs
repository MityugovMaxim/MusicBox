using UnityEngine;
using Zenject;

public class UIMainMenuChestsPage : UIMainMenuPage
{
	public override MainMenuPageType Type => MainMenuPageType.Chests;

	[SerializeField] UIGroup m_ContentGroup;
	[SerializeField] UIGroup m_LoaderGroup;

	[Inject] ChestsManager m_ChestsManager;

	protected override async void OnShowStarted()
	{
		m_ContentGroup.Hide(true);
		m_LoaderGroup.Show(true);
		
		bool instant = await m_ChestsManager.Activate();
		
		m_ContentGroup.Show(instant);
		m_LoaderGroup.Hide(instant);
	}
}
