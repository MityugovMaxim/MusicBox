using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControlButton : UIEntity
{
	[SerializeField] UnityEvent m_OnClick;
	[SerializeField] Color      m_Normal;
	[SerializeField] Color      m_Active;
	[SerializeField] Graphic    m_Graphic;
	[SerializeField] float      m_Duration;

	bool m_Pressed;
	bool m_Hovered;

	IEnumerator m_ColorRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		SetColor(m_Normal, true);
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		SetColor(m_Normal, true);
	}
	#endif

	public void OnPointerClick(PointerEventData _EventData)
	{
		m_OnClick?.Invoke();
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		SetColor(m_Active);
		
		m_Pressed = true;
		m_Hovered = true;
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		SetColor(m_Normal);
		
		m_Pressed = false;
		m_Hovered = false;
	}

	public void OnPointerEnter(PointerEventData _EventData)
	{
		if (!m_Pressed || m_Hovered)
			return;
		
		m_Hovered = true;
		
		SetColor(m_Active);
	}

	public void OnPointerExit(PointerEventData _EventData)
	{
		if (!m_Pressed || !m_Hovered)
			return;
		
		m_Hovered = false;
		
		SetColor(m_Normal);
	}

	void SetColor(Color _Color, bool _Instant = false)
	{
		if (m_ColorRoutine != null)
			StopCoroutine(m_ColorRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
			StartCoroutine(m_ColorRoutine = ColorRoutine(m_Graphic, _Color, m_Duration));
		else if (m_Graphic != null)
			m_Graphic.color = _Color;
	}

	static IEnumerator ColorRoutine(Graphic _Graphic, Color _Color, float _Duration)
	{
		if (_Graphic == null)
			yield break;
		
		Color source = _Graphic.color;
		Color target = _Color;
		
		if (source != target)
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Graphic.color = Color.Lerp(source, target, time / _Duration);
			}
		}
		
		_Graphic.color = target;
	}
}