using System;
using UnityEngine.EventSystems;

public class Preview : UIEntity, IPointerClickHandler
{
	public event Action OnClick;

	public void OnPointerClick(PointerEventData _EventData)
	{
		OnClick?.Invoke();
	}
}