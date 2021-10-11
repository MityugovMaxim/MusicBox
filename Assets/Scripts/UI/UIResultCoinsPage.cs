using UnityEngine;

public class UIResultCoinsPage : UIResultMenuPage
{
	public override ResultMenuPageType Type => ResultMenuPageType.Coins;

	[SerializeField] UICoinsLabel m_CoinsLabel;
	[SerializeField] UIGroup      m_ContinueGroup;

	string m_LevelID;

	public override void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
	}

	protected override void OnShowFinished()
	{
		m_ContinueGroup.Show();
	}
}