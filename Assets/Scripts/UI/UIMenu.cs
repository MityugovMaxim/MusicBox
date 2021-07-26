using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIMenu : UIEntity
{
	public bool Shown => m_Shown;

	[SerializeField] UIBlur m_Blur;

	protected Action CloseAction;

	Action m_ShowStarted;
	Action m_ShowFinished;
	Action m_HideStarted;
	Action m_HideFinished;

	CanvasGroup m_CanvasGroup;
	bool        m_Shown;
	IEnumerator m_Routine;

	protected override void Awake()
	{
		base.Awake();
		
		m_CanvasGroup                = GetComponent<CanvasGroup>();
		m_CanvasGroup.alpha          = 0;
		m_CanvasGroup.interactable   = false;
		m_CanvasGroup.blocksRaycasts = false;
	}

	public void Toggle(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (m_Shown)
			Hide(_Instant, _Started, _Finished);
		else
			Show(_Instant, _Started, _Finished);
	}

	public void Show(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (m_Shown)
			return;
		
		m_Shown        = true;
		m_ShowStarted  = _Started;
		m_ShowFinished = _Finished;
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_CanvasGroup.alpha          = 1;
			m_CanvasGroup.interactable   = true;
			m_CanvasGroup.blocksRaycasts = true;
			
			OnShowStarted();
			
			InvokeShowStarted();
			
			OnShowFinished();
			
			InvokeShowFinished();
		}
		else
		{
			m_Routine = ShowRoutine(0.2f);
			
			StartCoroutine(m_Routine);
		}
	}

	public void Hide(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (!m_Shown)
			return;
		
		m_Shown        = false;
		m_HideStarted  = _Started;
		m_HideFinished = _Finished;
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_CanvasGroup.alpha          = 0;
			m_CanvasGroup.interactable   = false;
			m_CanvasGroup.blocksRaycasts = false;
			
			OnHideStarted();
			
			InvokeHideStarted();
			
			OnHideFinished();
			
			InvokeHideFinished();
			
			InvokeCloseAction();
		}
		else
		{
			m_Routine = HideRoutine(0.2f);
			
			StartCoroutine(m_Routine);
		}
	}

	protected virtual void OnShowStarted() { }

	protected virtual void OnShowFinished() { }

	protected virtual void OnHideStarted() { }

	protected virtual void OnHideFinished() { }

	void InvokeCloseAction()
	{
		Action action = CloseAction;
		CloseAction = null;
		action?.Invoke();
	}

	IEnumerator ShowRoutine(float _Duration)
	{
		m_CanvasGroup.interactable   = true;
		m_CanvasGroup.blocksRaycasts = true;
		
		OnShowStarted();
		
		InvokeShowStarted();
		
		if (m_Blur != null)
		{
			m_Blur.Blur();
			yield return null;
		}
		
		float source = m_CanvasGroup.alpha;
		float target = 1;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				m_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		m_CanvasGroup.alpha = target;
		
		OnShowFinished();
		
		InvokeShowFinished();
	}

	IEnumerator HideRoutine(float _Duration)
	{
		OnHideStarted();
		
		InvokeHideStarted();
		
		float source = m_CanvasGroup.alpha;
		float target = 0;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				m_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		m_CanvasGroup.alpha = target;
		
		m_CanvasGroup.interactable   = false;
		m_CanvasGroup.blocksRaycasts = false;
		
		OnHideFinished();
		
		InvokeHideFinished();
		
		InvokeCloseAction();
	}

	void InvokeShowStarted()
	{
		Action action = m_ShowStarted;
		m_ShowStarted = null;
		action?.Invoke();
	}

	void InvokeShowFinished()
	{
		Action action = m_ShowFinished;
		m_ShowFinished = null;
		action?.Invoke();
	}

	void InvokeHideStarted()
	{
		Action action = m_HideStarted;
		m_HideStarted = null;
		action?.Invoke();
	}

	void InvokeHideFinished()
	{
		Action action = m_HideFinished;
		m_HideFinished = null;
		action?.Invoke();
	}
}