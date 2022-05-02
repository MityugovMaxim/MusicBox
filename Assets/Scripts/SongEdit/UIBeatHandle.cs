using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIBeatHandle : UIEntity, IDragHandler
{
	[Inject] UIBeat m_Beat;

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		Vector2 delta = RectTransform.InverseTransformVector(_EventData.delta);
		
		Rect  rect  = m_Beat.GetLocalRect();
		float speed = m_Beat.Duration / rect.height;
		m_Beat.Origin -= delta.y * speed;
	}
}