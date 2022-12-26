using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.ChestMenu)]
public class UIChestMenu : UIMenu
{
	[SerializeField] UIChestReward m_Reward;
	[SerializeField] Button        m_Button;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(Close);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Subscribe(Close);
	}

	public void Setup(string _ChestID, ChestReward _Reward)
	{
		m_Reward.Setup(_ChestID, _Reward);
	}

	protected override async void OnShowFinished()
	{
		base.OnShowFinished();
		
		m_Button.interactable = false;
		
		await m_Reward.PlayAsync();
		
		m_Button.interactable = true;
	}

	void Close() => Hide();
}
