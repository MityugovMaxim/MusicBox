using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIControl : Graphic, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField, Range(0, 1)] float m_SourceAlpha = 0.5f;
	[SerializeField, Range(0, 1)] float m_TargetAlpha = 1;

	[SerializeField] float             m_Duration  = 0.2f;
	[SerializeField] float             m_FadeDelay = 2;
	[SerializeField] UIControlButton[] m_Buttons;

	bool        m_Detect;
	bool        m_Active;
	float       m_FadeTime;
	CanvasGroup m_CanvasGroup;

	IEnumerator m_FadeRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		FadeOut(true);
	}

	void Update()
	{
		if (m_Active && Time.time >= m_FadeTime)
		{
			m_Active = false;
			
			FadeOut();
		}
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (Application.isPlaying)
			return;
		
		UnityEditor.EditorApplication.delayCall += () =>
		{
			if (this == null)
				return;
			
			FadeOut(true);
		};
	}
	#endif

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	void FadeIn(bool _Instant = false)
	{
		if (m_FadeRoutine != null)
			StopCoroutine(m_FadeRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
			StartCoroutine(m_FadeRoutine = FadeRoutine(m_TargetAlpha, m_Duration));
		else
			CanvasGroup.alpha = m_TargetAlpha;
	}

	void FadeOut(bool _Instant = false)
	{
		foreach (UIControlButton button in m_Buttons)
		{
			if (button != null)
				button.OnPointerUp(null);
		}
		
		if (m_FadeRoutine != null)
			StopCoroutine(m_FadeRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
			StartCoroutine(m_FadeRoutine = FadeRoutine(m_SourceAlpha, m_Duration));
		else
			CanvasGroup.alpha = m_SourceAlpha;
	}

	void PassEvent(UIEntity _Button, PointerEventData _EventData, Action<PointerEventData> _Event, bool _Intersect)
	{
		if (!m_Active || _Button == null || _Event == null)
			return;
		
		Rect rect = _Button.RectTransform.rect;
		
		bool interact = RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_Button.RectTransform,
			_EventData.position,
			_EventData.enterEventCamera,
			out Vector2 position
		);
		
		if (interact && (!_Intersect || rect.Contains(position)))
			_Event(_EventData);
	}

	IEnumerator FadeRoutine(float _Alpha, float _Duration)
	{
		float source = CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		CanvasGroup.alpha = target;
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData _EventData)
	{
		foreach (UIControlButton button in m_Buttons)
			PassEvent(button, _EventData, button.OnPointerClick, true);
		
		if (m_Detect)
		{
			m_Detect = false;
			m_Active = true;
			
			FadeIn();
		}
		
		m_FadeTime = Time.time + m_FadeDelay;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Detect = !m_Active;
		
		foreach (UIControlButton button in m_Buttons)
			PassEvent(button, _EventData, button.OnPointerDown, true);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		foreach (UIControlButton button in m_Buttons)
			PassEvent(button, _EventData, button.OnPointerUp, true);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		foreach (UIControlButton button in m_Buttons)
			PassEvent(button, _EventData, button.OnPointerEnter, true);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData  _EventData)
	{
		foreach (UIControlButton button in m_Buttons)
			PassEvent(button, _EventData, button.OnPointerExit, false);
	}
}
