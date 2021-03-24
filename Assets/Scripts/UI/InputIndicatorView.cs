using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public abstract class InputIndicatorView : UIBehaviour
{
	protected RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	protected CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	RectTransform m_RectTransform;
	CanvasGroup   m_CanvasGroup;

	public abstract void Begin();

	public abstract void Process(float _Time);

	public abstract void Complete(Action _Callback = null);

	public abstract void Success(Action _Callback = null);

	public abstract void Fail(Action _Callback = null);
}