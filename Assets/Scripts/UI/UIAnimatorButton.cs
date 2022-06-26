using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	bool m_Pressed;

	protected abstract void OnNormal();

	protected abstract void OnPress();

	protected abstract void OnClick();

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Pressed = true;
		
		OnPress();
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		m_Pressed = false;
		
		if (_EventData.dragging)
			OnNormal();
		else
			OnClick();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		if (m_Pressed)
			OnPress();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		if (m_Pressed)
			OnNormal();
	}
}

public class UIOverlayButton : UIButton
{
	[SerializeField] UIGroup m_Overlay;

	protected override void OnNormal()
	{
		m_Overlay.Hide();
	}

	protected override void OnPress()
	{
		m_Overlay.Show();
	}

	protected override void OnClick()
	{
		m_Overlay.Hide();
	}
}

[RequireComponent(typeof(Animator))]
public class UIAnimatorButton : UIButton
{
	static readonly int m_NormalParameterID = Animator.StringToHash("Normal");
	static readonly int m_PressParameterID  = Animator.StringToHash("Press");
	static readonly int m_ClickParameterID  = Animator.StringToHash("Click");

	Animator m_Animator;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnNormal()
	{
		m_Animator.SetTrigger(m_NormalParameterID);
	}

	protected override void OnPress()
	{
		m_Animator.SetTrigger(m_PressParameterID);
	}

	protected override void OnClick()
	{
		m_Animator.SetTrigger(m_ClickParameterID);
	}
}