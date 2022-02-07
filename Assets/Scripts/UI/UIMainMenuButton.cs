using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(Animator))]
public class UIMainMenuButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	[Serializable]
	public class MainMenuButtonEvent : UnityEvent<MainMenuPageType> { }

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

	[SerializeField] MainMenuPageType    m_PageType;
	[SerializeField] MainMenuButtonEvent m_Click;

	HapticProcessor m_HapticProcessor;

	Animator m_Animator;

	bool m_Pressed;

	[Inject]
	public void Construct(HapticProcessor _HapticProcessor)
	{
		m_HapticProcessor = _HapticProcessor;
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
		Animator.SetBool(m_PressedParameterID, false);
		
		m_Click?.Invoke(m_PageType);
		
		m_HapticProcessor.Process(Haptic.Type.ImpactSoft);
	}
}