using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UITicketEditMenu : UIMenu
{
	ProfileTicketType             Type    { get; set; }
	TicketCreateRequest.GroupType Group   { get; set; }
	long                          Amount  { get; set; }
	string                        UserID  { get; set; }
	string                        Region  { get; set; }
	int                           Days    { get; set; }
	int                           Hours   { get; set; }
	int                           Minutes { get; set; }

	[SerializeField] UIEnumField    m_TypeField;
	[SerializeField] UIEnumField    m_GroupField;
	[SerializeField] UILongField    m_AmountField;
	[SerializeField] UIStringField  m_UserIDField;
	[SerializeField] UIStringField  m_RegionField;
	[SerializeField] UIIntegerField m_DaysField;
	[SerializeField] UIIntegerField m_HoursField;
	[SerializeField] UIIntegerField m_MinutesField;
	[SerializeField] Button         m_CreateButton;

	[Inject] MenuProcessor m_MenuProcessor;

	protected override void Awake()
	{
		base.Awake();
		
		m_TypeField.Setup(this, nameof(Type));
		m_GroupField.Setup(this, nameof(Group));
		m_AmountField.Setup(this, nameof(Amount));
		m_UserIDField.Setup(this, nameof(UserID));
		m_RegionField.Setup(this, nameof(Region));
		m_DaysField.Setup(this, nameof(Days));
		m_HoursField.Setup(this, nameof(Hours));
		m_MinutesField.Setup(this, nameof(Minutes));
		
		m_GroupField.OnValueChanged += OnGroupChanged;
		
		m_CreateButton.Subscribe(Create);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_GroupField.OnValueChanged -= OnGroupChanged;
		
		m_CreateButton.Unsubscribe(Create);
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		Type    = ProfileTicketType.ProductDiscount;
		Group   = TicketCreateRequest.GroupType.All;
		UserID  = string.Empty;
		Region  = string.Empty;
		Amount  = 5;
		Days    = 1;
		Hours   = 6;
		Minutes = 30;
		
		m_TypeField.Restore();
		m_GroupField.Restore();
		m_AmountField.Restore();
		m_UserIDField.Restore();
		m_RegionField.Restore();
		m_DaysField.Restore();
		m_HoursField.Restore();
		m_MinutesField.Restore();
	}

	async void Create()
	{
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		long expirationTimestamp = TimeUtility.GetTimestamp(Days, Hours, Minutes);
		
		TicketCreateRequest request = new TicketCreateRequest(
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
			await m_MenuProcessor.Hide(MenuType.TicketCreateMenu);
		}
		else
		{
			await m_MenuProcessor.ErrorAsync(
				"ticket_create",
				"ticket_create_menu",
				"Create ticket failed",
				"Something went wrong. Check the parameters and try again"
			);
		}
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	void OnGroupChanged()
	{
		m_UserIDField.gameObject.SetActive(Group == TicketCreateRequest.GroupType.User);
		m_RegionField.gameObject.SetActive(Group == TicketCreateRequest.GroupType.Region);
	}
}
