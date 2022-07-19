using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIColorScheme : UIEntity, IPointerClickHandler
{
	public bool Marker => m_Marker.activeSelf;

	[SerializeField] GameObject m_Marker;
	[SerializeField] Graphic    m_BackgroundPrimary;
	[SerializeField] Graphic    m_BackgroundSecondary;
	[SerializeField] Graphic    m_ForegroundPrimary;
	[SerializeField] Graphic    m_ForegroundSecondary;

	Action<Color, Color, Color, Color> m_Callback;

	public void Setup(
		bool                               _Marker,
		Color                              _BackgroundPrimary,
		Color                              _BackgroundSecondary,
		Color                              _ForegroundPrimary,
		Color                              _ForegroundSecondary,
		Action<Color, Color, Color, Color> _Callback
	)
	{
		m_Marker.SetActive(_Marker);
		m_BackgroundPrimary.color   = _BackgroundPrimary;
		m_BackgroundSecondary.color = _BackgroundSecondary;
		m_ForegroundPrimary.color   = _ForegroundPrimary;
		m_ForegroundSecondary.color = _ForegroundSecondary;
		m_Callback                  = _Callback;
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		m_Callback?.Invoke(
			m_BackgroundPrimary.color,
			m_BackgroundSecondary.color,
			m_ForegroundPrimary.color,
			m_ForegroundSecondary.color
		);
	}
}