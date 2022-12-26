using UnityEngine;
using Zenject;

public class UISeasonItemAction : UISeasonItemEntity
{
	[SerializeField] UIButton m_Button;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_Button.Subscribe(ProcessAction);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Button.Unsubscribe(ProcessAction);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }

	async void ProcessAction()
	{
		if (Mode == SeasonItemMode.Paid && !SeasonsManager.HasPass(SeasonID))
		{
			UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
			
			if (productMenu == null)
				return;
			
			productMenu.Setup(SeasonID);
			
			productMenu.Show();
		}
		else if (SeasonsManager.IsItemAvailable(SeasonID, Level, Mode))
		{
			await SeasonsManager.Collect(SeasonID, Level, Mode);
		}
	}
}
