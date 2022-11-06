using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.VoucherCreateMenu)]
public class UIVoucherCreateMenu : UIMenu
{
	public ProfileVoucherType             Type    { get; set; }
	public VoucherCreateRequest.GroupType Group   { get; set; }
	public string                         UserID  { get; set; }
	public string                         Region  { get; set; }
	public long                           Amount  { get; set; }
	public int                            Days    { get; set; }
	public int                            Hours   { get; set; }
	public int                            Minutes { get; set; }

	[SerializeField] UIEnumField    m_TypeField;
	[SerializeField] UIEnumField    m_GroupField;
	[SerializeField] UIStringField  m_UserIDField;
	[SerializeField] UIStringField  m_RegionField;
	[SerializeField] UILongField    m_AmountField;
	[SerializeField] UIIntegerField m_DaysField;
	[SerializeField] UIIntegerField m_HoursField;
	[SerializeField] UIIntegerField m_MinutesField;
	[SerializeField] Button         m_CreateButton;
	[SerializeField] Button         m_BackButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_GroupField.OnValueChanged += ProcessGroup;
		
		m_CreateButton.Subscribe(Create);
		m_BackButton.Subscribe(Back);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_GroupField.OnValueChanged -= ProcessGroup;
		
		m_CreateButton.Unsubscribe(Create);
		m_BackButton.Unsubscribe(Back);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Type    = ProfileVoucherType.ProductDiscount;
		Group   = VoucherCreateRequest.GroupType.All;
		UserID  = string.Empty;
		Region  = string.Empty;
		Amount  = 5;
		Days    = 1;
		Hours   = 6;
		Minutes = 30;
		
		m_TypeField.Setup(this, nameof(Type));
		m_GroupField.Setup(this, nameof(Group));
		m_AmountField.Setup(this, nameof(Amount));
		m_UserIDField.Setup(this, nameof(UserID));
		m_RegionField.Setup(this, nameof(Region));
		m_DaysField.Setup(this, nameof(Days));
		m_HoursField.Setup(this, nameof(Hours));
		m_MinutesField.Setup(this, nameof(Minutes));
		
		ProcessGroup();
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
		
		long expirationTimestamp = TimeUtility.GetTimestamp(Days, Hours, Minutes);
		
		VoucherCreateRequest request = new VoucherCreateRequest(
			Type,
			Group,
			UserID,
			Region,
			Amount,
			expirationTimestamp
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

	void ProcessGroup()
	{
		m_UserIDField.gameObject.SetActive(Group == VoucherCreateRequest.GroupType.User);
		m_RegionField.gameObject.SetActive(Group == VoucherCreateRequest.GroupType.Region);
	}
}
