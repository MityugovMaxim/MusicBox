using UnityEngine.EventSystems;

public abstract class UIButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public readonly DynamicDelegate Action = new DynamicDelegate();

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
		{
			OnNormal();
		}
		else
		{
			OnClick();
			Action?.Invoke();
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
