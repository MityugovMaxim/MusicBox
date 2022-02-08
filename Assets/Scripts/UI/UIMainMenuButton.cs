using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIMainMenuButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public MainMenuPageType PageType => m_PageType;

	Animator Animator
	{
		get
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
				m_Animator.keepAnimatorControllerStateOnDisable = true;
			}
			return m_Animator;
		}
	}

	static readonly int m_EnabledParameterID  = Animator.StringToHash("Enable");
	static readonly int m_DisabledParameterID = Animator.StringToHash("Disable");
	static readonly int m_PressedParameterID  = Animator.StringToHash("Pressed");
	static readonly int m_InstantParameterID  = Animator.StringToHash("Instant");

	[SerializeField] MainMenuPageType m_PageType;

	MenuProcessor      m_MenuProcessor;
	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	Animator m_Animator;

	bool m_Pressed;

	[Inject]
	public void Construct(
		MenuProcessor      _MenuProcessor,
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_MenuProcessor      = _MenuProcessor;
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Toggle(bool _Value, bool _Instant = false)
	{
		Animator.SetBool(m_InstantParameterID, _Instant);
		Animator.ResetTrigger(m_EnabledParameterID);
		Animator.ResetTrigger(m_DisabledParameterID);
		Animator.SetTrigger(_Value ? m_EnabledParameterID : m_DisabledParameterID);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_HapticProcessor.Process(Haptic.Type.Selection);
		
		m_Pressed = true;
		
		Animator.SetBool(m_PressedParameterID, m_Pressed);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		m_Pressed = false;
		
		Animator.SetBool(m_PressedParameterID, m_Pressed);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		Animator.SetBool(m_PressedParameterID, m_Pressed);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		Animator.SetBool(m_PressedParameterID, false);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_StatisticProcessor.LogMainMenuPageSelect(m_PageType);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
		
		UIMainMenu mainMenu = m_MenuProcessor.GetMenu<UIMainMenu>();
		if (mainMenu != null)
			mainMenu.Select(m_PageType);
		
		Animator.SetBool(m_PressedParameterID, false);
	}
}