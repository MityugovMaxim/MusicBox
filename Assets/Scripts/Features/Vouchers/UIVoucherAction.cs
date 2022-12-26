using UnityEngine;
using Zenject;

public class UIVoucherAction : UIVoucherEntity
{
	[SerializeField] UIButton m_Button;

	[Inject] UrlProcessor    m_UrlProcessor;
	[Inject] MenuProcessor   m_MenuProcessor;

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

	async void ProcessAction()
	{
		if (string.IsNullOrEmpty(VoucherID))
			return;
		
		string link;
		switch (VouchersManager.GetType(VoucherID))
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
			case VoucherType.SeasonsBoost:
				link = "audiobox://seasons";
				break;
			default: return;
		}
		
		await m_MenuProcessor.Show(MenuType.BlockMenu);
		
		await m_UrlProcessor.ProcessURL(link);
		
		await m_MenuProcessor.Hide(MenuType.BlockMenu);
	}

	protected override void Subscribe() { }

	protected override void Unsubscribe() { }

	protected override void ProcessData() { }
}
