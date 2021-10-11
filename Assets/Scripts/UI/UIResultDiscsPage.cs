using UnityEngine;
using Zenject;

public class UIResultDiscsPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Discs;

	[SerializeField] UIProfile       m_Profile;
	[SerializeField] UIDiscsProgress m_DiscsProgress;
	[SerializeField] UIGroup         m_ContinueGroup;

	MenuProcessor m_MenuProcessor;

	string m_LevelID;

	[Inject]
	public void Construct(MenuProcessor _MenuProcessor)
	{
		m_MenuProcessor = _MenuProcessor;
	}

	public override void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_DiscsProgress.Setup();
	}

	public void Continue()
	{
		UIResultMenu resultMenu = m_MenuProcessor.GetMenu<UIResultMenu>();
		if (resultMenu != null)
			resultMenu.Select(ResultMenuPageType.Coins);
	}

	protected override async void OnShowFinished()
	{
		await m_DiscsProgress.Play();
		
		m_ContinueGroup.Show();
	}
}