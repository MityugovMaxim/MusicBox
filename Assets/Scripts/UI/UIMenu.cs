using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

public class MenuAttribute : Attribute
{
	public MenuType MenuType { get; }

	public MenuAttribute(MenuType _MenuType)
	{
		MenuType = _MenuType;
	}
}

[RequireComponent(typeof(CanvasGroup))]
public class UIMenu : UIEntity
{
	[Preserve]
	public class Factory : PlaceholderFactory<UIMenu, UIMenu> { }

	public bool Shown { get; private set; }

	protected float ShowDuration => m_ShowDuration;
	protected float HideDuration => m_HideDuration;

	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField] UIBlur m_Blur;

	protected Action CloseAction;

	[SerializeField] float m_ShowDuration = 0.2f;
	[SerializeField] float m_HideDuration = 0.2f;

	Action m_ShowStarted;
	Action m_ShowFinished;
	Action m_HideStarted;
	Action m_HideFinished;

	CanvasGroup m_CanvasGroup;
	IEnumerator m_Routine;

	protected override void Awake()
	{
		base.Awake();
		
		Shown = false;
		
		Hide(true);
	}

	public void Show(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (Shown)
		{
			_Started?.Invoke();
			_Finished?.Invoke();
			return;
		}
		
		Shown          = true;
		m_ShowStarted  = _Started;
		m_ShowFinished = _Finished;
		
		gameObject.SetActive(true);
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			CanvasGroup.interactable   = true;
			CanvasGroup.blocksRaycasts = true;
			
			OnShowStarted();
			
			InvokeShowStarted();
			
			InstantShow(CanvasGroup);
			
			OnShowFinished();
			
			InvokeShowFinished();
		}
		else
		{
			m_Routine = ShowRoutine(CanvasGroup, m_ShowDuration);
			
			StartCoroutine(m_Routine);
		}
	}

	public void Hide(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (!Shown)
		{
			_Started?.Invoke();
			_Finished?.Invoke();
			return;
		}
		
		Shown          = false;
		m_HideStarted  = _Started;
		m_HideFinished = _Finished;
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			CanvasGroup.interactable   = false;
			CanvasGroup.blocksRaycasts = false;
			
			OnHideStarted();
			
			InvokeHideStarted();
			
			InstantHide(CanvasGroup);
			
			OnHideFinished();
			
			InvokeHideFinished();
			
			InvokeCloseAction();
			
			gameObject.SetActive(false);
		}
		else
		{
			m_Routine = HideRoutine(CanvasGroup, m_HideDuration);
			
			StartCoroutine(m_Routine);
		}
	}

	protected virtual void OnShowStarted() { }

	protected virtual void OnShowFinished() { }

	protected virtual void OnHideStarted() { }

	protected virtual void OnHideFinished() { }

	IEnumerator ShowRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		_CanvasGroup.interactable = true;
		
		OnShowStarted();
		
		InvokeShowStarted();
		
		if (m_Blur != null)
		{
			m_Blur.Blur();
			yield return null;
		}
		
		yield return ShowAnimation(_CanvasGroup, _Duration);
		
		_CanvasGroup.blocksRaycasts = true;
		
		OnShowFinished();
		
		InvokeShowFinished();
	}

	protected virtual IEnumerator ShowAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = 1;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}

	IEnumerator HideRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		OnHideStarted();
		
		InvokeHideStarted();
		
		yield return HideAnimation(_CanvasGroup, _Duration);
		
		_CanvasGroup.interactable   = false;
		_CanvasGroup.blocksRaycasts = false;
		
		OnHideFinished();
		
		InvokeHideFinished();
		
		InvokeCloseAction();
		
		gameObject.SetActive(false);
	}

	protected virtual IEnumerator HideAnimation(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = 0;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}

	protected virtual void InstantShow(CanvasGroup _CanvasGroup)
	{
		if (_CanvasGroup != null)
			_CanvasGroup.alpha = 1;
	}

	protected virtual void InstantHide(CanvasGroup _CanvasGroup)
	{
		if (_CanvasGroup != null)
			_CanvasGroup.alpha = 0;
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

	protected void InvokeCloseAction()
	{
		Action action = CloseAction;
		CloseAction = null;
		action?.Invoke();
	}
}