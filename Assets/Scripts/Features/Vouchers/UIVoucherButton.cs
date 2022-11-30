using Zenject;

public class UIVoucherButton : UIOverlayButton
{
	public string VoucherID { get; set; }

	[Inject] VouchersManager m_VouchersManager;
	[Inject] UrlProcessor    m_UrlProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

	protected override async void OnClick()
	{
		base.OnClick();
		
		if (string.IsNullOrEmpty(VoucherID))
			return;
		
		string link;
		switch (m_VouchersManager.GetType(VoucherID))
		{
			case VoucherType.ProductDiscount:
				link = "audiobox://store";
				break;
			case VoucherType.SongDiscount:
				link = "audiobox://songs";
				break;
			case VoucherType.ChestDiscount:
				link = "audiobox://chests";
				break;
			default: return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_UrlProcessor.ProcessURL(link);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}
}