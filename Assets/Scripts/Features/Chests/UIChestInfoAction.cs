using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIChestInfoAction : UIEntity
{
	[SerializeField] Button m_InfoButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_InfoButton.Subscribe(Info);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_InfoButton.Subscribe(Info);
	}

	void Info()
	{
		// TODO: Show Chest Info Menu
		// UIChestInfoMenu chestInfoMenu = m_MenuProcessor.GetMenu<UIChestInfoMenu>();
		//
		// if (chestInfoMenu == null)
		// 	return;
		//
		// chestInfoMenu.Setup(m_Rank);
		// chestInfoMenu.Show();
	}
}
