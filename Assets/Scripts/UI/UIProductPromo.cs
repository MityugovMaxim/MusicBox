using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIProductPromo : UIEntity, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Preserve]
	public class Pool : UIEntityPool<UIProductPromo> { }

	static readonly int m_NormalParameterID = Animator.StringToHash("Normal");
	static readonly int m_PressParameterID  = Animator.StringToHash("Press");
	static readonly int m_ClickParameterID  = Animator.StringToHash("Click");

	[SerializeField] UIProductImage m_Image;
	[SerializeField] UIProductLabel m_Label;
	[SerializeField] UIProductPrice m_Price;

	[Inject] MenuProcessor      m_MenuProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;

	Animator m_Animator;
	bool     m_Pressed;
	bool     m_Clicked;

	string m_ProductID;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	public void Setup(string _ProductID)
	{
		m_ProductID = _ProductID;
		
		m_Image.Setup(m_ProductID);
		m_Label.Setup(m_ProductID);
		m_Price.Setup(m_ProductID);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Pressed = true;
		m_Clicked = false;
		
		m_Animator.SetTrigger(m_PressParameterID);
	}

	async void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_Clicked = true;
		
		m_StatisticProcessor.LogMainMenuPromoClick(m_ProductID);
		
		m_Animator.SetTrigger(m_ClickParameterID);
		
		UIProductMenu productMenu = m_MenuProcessor.GetMenu<UIProductMenu>();
		if (productMenu != null)
			productMenu.Setup(m_ProductID);
		
		await m_MenuProcessor.Show(MenuType.ProductMenu);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(MainMenuPageType.Store, true);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		if (m_Pressed)
			m_Animator.SetTrigger(m_PressParameterID);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		m_Pressed = false;
		
		if (m_Clicked)
			return;
		
		m_Animator.SetTrigger(m_NormalParameterID);
	}
}