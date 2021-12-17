using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Menu(MenuType.RewardMenu)]
public class UIRewardMenu : UIMenu
{
	[SerializeField] UIReward      m_Reward;
	[SerializeField] UIRemoteImage m_Icon;
	[SerializeField] TMP_Text      m_Title;
	[SerializeField] TMP_Text      m_Description;
	[SerializeField] UIGroup       m_Dialog;

	UIEntity m_Entity;

	public void Setup(
		UIEntity     _Entity,
		Task<Sprite> _Sprite,
		string       _Title,
		string       _Description
	)
	{
		if (m_Reward != null)
			m_Reward.Setup(_Entity);
		
		if (m_Icon != null)
			m_Icon.Load(_Sprite);
		
		if (m_Title != null)
			m_Title.text = _Title;
		
		if (m_Description != null)
			m_Description.text = _Description;
	}

	public async Task Play()
	{
		if (m_Dialog != null)
			m_Dialog.Hide();
		
		if (m_Reward != null)
			await m_Reward.Play();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		if (m_Dialog != null)
			m_Dialog.Show(true);
	}
}