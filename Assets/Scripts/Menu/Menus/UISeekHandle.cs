using AudioBox.ASF;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISeekHandle : UIEntity, IDragHandler
{
	protected ASFPlayer Player => m_Player;

	[SerializeField] ASFPlayer m_Player;

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		if (m_Player.State != ASFPlayerState.Stop)
			return;
		
		_EventData.Use();
		
		int touchCount = _EventData.currentInputModule.input.touchCount;
		
		Vector2 delta = RectTransform.InverseTransformVector(_EventData.delta);
		Rect    rect  = m_Player.GetLocalRect();
		float   speed = m_Player.Duration / rect.height;
		m_Player.Time -= delta.y * speed * (touchCount > 1 ? 10 : 1);
	}
}