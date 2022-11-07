using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.VoucherCreateMenu)]
public class UIVoucherCreateMenu : UIMenu
{
	public VoucherType  Type    { get; set; }
	public VoucherGroup Group   { get; set; }
	public long         Amount  { get; set; }
	public int          Days    { get; set; }
	public int          Hours   { get; set; }
	public int          Minutes { get; set; }
	public List<string> IDs     { get; set; }

	[SerializeField] UIEnumField       m_TypeField;
	[SerializeField] UIEnumField       m_GroupField;
	[SerializeField] UILongField       m_AmountField;
	[SerializeField] UIIntegerField    m_DaysField;
	[SerializeField] UIIntegerField    m_HoursField;
	[SerializeField] UIIntegerField    m_MinutesField;
	[SerializeField] UIStringListField m_IDsField;
	[SerializeField] Button            m_CreateButton;
	[SerializeField] Button            m_BackButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_CreateButton.Subscribe(Create);
		m_BackButton.Subscribe(Back);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CreateButton.Unsubscribe(Create);
		m_BackButton.Unsubscribe(Back);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Type    = VoucherType.ProductDiscount;
		Group   = VoucherGroup.All;
		Amount  = 5;
		Days    = 1;
		Hours   = 6;
		Minutes = 30;
		IDs     = new List<string>();
		
		m_TypeField.Setup(this, nameof(Type));
		m_GroupField.Setup(this, nameof(Group));
		m_AmountField.Setup(this, nameof(Amount));
		m_IDsField.Setup(this, nameof(IDs));
		m_DaysField.Setup(this, nameof(Days));
		m_HoursField.Setup(this, nameof(Hours));
		m_MinutesField.Setup(this, nameof(Minutes));
	}

	async void Create()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"voucher_create",
			"Create Voucher",
			"Are you sure want to create new voucher?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		long expiration = Days > 0 || Hours > 0 || Minutes > 0 ? TimeUtility.GetTimestamp(Days, Hours, Minutes) : 0;
		
		VoucherCreateRequest request = new VoucherCreateRequest(
			Type,
			Group,
			Amount,
			IDs,
			expiration
		);
		
		bool success = await request.SendAsync();
		
		if (success)
		{
			await HideAsync();
		}
		else
		{
			await m_MenuProcessor.ErrorAsync(
				"voucher_create",
				"voucher_create_menu",
				"Create voucher failed",
				"Something went wrong. Check the parameters and try again"
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void Back() => Hide();
}
