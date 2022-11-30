using Zenject;

public class UISeasonItemButton : UIOverlayButton
{
	[Inject] SeasonsManager m_SeasonsManager;
	[Inject] MenuProcessor  m_MenuProcessor;

	string m_SeasonID;
	string m_ItemID;

	public void Setup(string _SeasonID, string _ItemID)
	{
		m_SeasonID = _SeasonID;
		m_ItemID   = _ItemID;
	}

	protected override async void OnClick()
	{
		base.OnClick();
		
		bool available = m_SeasonsManager.IsItemAvailable(m_SeasonID, m_ItemID);
		bool pass      = m_SeasonsManager.HasPass(m_SeasonID);
		bool free      = m_SeasonsManager.IsFreeItem(m_SeasonID, m_ItemID);
		bool paid      = m_SeasonsManager.IsPaidItem(m_SeasonID, m_ItemID);
		
		if (!available)
			return;
		
		if (free || paid && pass)
		{
			await m_SeasonsManager.Collect(m_SeasonID, m_ItemID);
			return;
		}
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		
		if (productMenu == null)
			return;
		
		productMenu.Setup(m_SeasonID);
		
		productMenu.Show();
	}
}