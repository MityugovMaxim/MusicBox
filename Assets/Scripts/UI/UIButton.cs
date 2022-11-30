using System;
using UnityEngine.EventSystems;

public abstract class UIButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	bool m_Pressed;

	Action m_Action;

	public void Subscribe(Action _Action) => m_Action += _Action;

	public void Unsubscribe(Action _Action) => m_Action -= _Action;

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
		{
			OnNormal();
		}
		else
		{
			OnClick();
			m_Action?.Invoke();
		}
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