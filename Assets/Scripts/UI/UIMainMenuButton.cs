using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class UIMainMenuButton : UIEntity, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	[Serializable]
	public class MainMenuButtonEvent : UnityEvent<MainMenuPageType> { }

	public MainMenuPageType PageType => m_PageType;

	static readonly int m_EnabledParameterID  = Animator.StringToHash("Enabled");
	static readonly int m_DisabledParameterID = Animator.StringToHash("Disabled");
	static readonly int m_PressedParameterID  = Animator.StringToHash("Pressed");
	static readonly int m_InstantParameterID  = Animator.StringToHash("Instant");

	[SerializeField] MainMenuPageType    m_PageType;
	[SerializeField] MainMenuButtonEvent m_Click;

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
		
		m_Animator.keepAnimatorControllerStateOnDisable = true;
	}

	public void Toggle(bool _Value, bool _Instant = false)
	{
		m_Animator.SetBool(m_InstantParameterID, _Instant);
		m_Animator.SetTrigger(_Value ? m_EnabledParameterID : m_DisabledParameterID);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Animator.SetBool(m_PressedParameterID, true);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		m_Animator.SetBool(m_PressedParameterID, true);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		m_Animator.SetBool(m_PressedParameterID, false);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_Animator.SetBool(m_PressedParameterID, false);
		
		m_Click?.Invoke(m_PageType);
	}
}