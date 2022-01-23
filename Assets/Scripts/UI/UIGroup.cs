using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIGroup : UIEntity
{
	public bool Shown { get; private set; }

	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField] bool  m_Interactable;
	[SerializeField] float m_ShowDuration = 0.2f;
	[SerializeField] float m_HideDuration = 0.2f;

	CanvasGroup m_CanvasGroup;

	Action m_ShowStarted;
	Action m_ShowFinished;
	Action m_HideStarted;
	Action m_HideFinished;

	IEnumerator m_DisplayRoutine;

	TaskCompletionSource<bool> m_TaskSource;

	public Task ShowAsync(bool _Instant = false)
	{
		m_TaskSource?.TrySetResult(false);
		
		m_TaskSource = new TaskCompletionSource<bool>();
		
		Show(
			_Instant,
			null,
			() => m_TaskSource?.TrySetResult(true)
		);
		
		return m_TaskSource.Task;
	}

	public Task HideAsync(bool _Instant = false)
	{
		m_TaskSource?.TrySetResult(false);
		
		m_TaskSource = new TaskCompletionSource<bool>();
		
		Hide(
			_Instant,
			null,
			() => m_TaskSource?.TrySetResult(true)
		);
		
		return m_TaskSource.Task;
	}

	public void Show(bool _Instant = false, Action _Started = null, Action _Finished = null)
	{
		if (Shown)
		{
			_Started?.Invoke();
			_Finished?.Invoke();
			return;
		}
		
		if (m_DisplayRoutine != null)
			StopCoroutine(m_DisplayRoutine);
		
		InvokeHideStarted();
		InvokeHideFinished();
		
		gameObject.SetActive(true);
		
		Shown          = true;
		m_ShowStarted  = _Started;
		m_ShowFinished = _Finished;
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_DisplayRoutine = ShowRoutine(CanvasGroup, m_ShowDuration);
			
			StartCoroutine(m_DisplayRoutine);
		}
		else
		{
			OnShowStarted();
			
			CanvasGroup.alpha          = 1;
			CanvasGroup.interactable   = m_Interactable;
			CanvasGroup.blocksRaycasts = m_Interactable;
			
			InvokeShowStarted();
			
			OnShowFinished();
			
			InvokeShowFinished();
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
		
		if (m_DisplayRoutine != null)
			StopCoroutine(m_DisplayRoutine);
		
		InvokeShowStarted();
		InvokeShowFinished();
		
		if (!gameObject.activeSelf)
			gameObject.SetActive(true);
		
		Shown          = false;
		m_HideStarted  = _Started;
		m_HideFinished = _Finished;
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_DisplayRoutine = HideRoutine(CanvasGroup, m_HideDuration);
			
			StartCoroutine(m_DisplayRoutine);
		}
		else
		{
			OnHideStarted();
			
			InvokeHideStarted();
			
			CanvasGroup.alpha          = 0;
			CanvasGroup.blocksRaycasts = false;
			CanvasGroup.interactable   = false;
			
			OnHideFinished();
			
			InvokeHideFinished();
			
			if (gameObject.activeSelf)
				gameObject.SetActive(false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (Shown)
		{
			CanvasGroup.alpha          = 1;
			CanvasGroup.interactable   = m_Interactable;
			CanvasGroup.blocksRaycasts = m_Interactable;
		}
		else
		{
			CanvasGroup.alpha          = 0;
			CanvasGroup.interactable   = false;
			CanvasGroup.blocksRaycasts = false;
		}
	}

	protected virtual void OnShowStarted() { }

	protected virtual void OnShowFinished() { }

	protected virtual void OnHideStarted() { }

	protected virtual void OnHideFinished() { }

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

	IEnumerator ShowRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		OnShowStarted();
		
		InvokeShowStarted();
		
		_CanvasGroup.interactable   = m_Interactable;
		_CanvasGroup.blocksRaycasts = m_Interactable;
		
		yield return StartCoroutine(ShowAnimationRoutine(_CanvasGroup, _Duration));
		
		OnShowFinished();
		
		InvokeShowFinished();
	}

	IEnumerator HideRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		OnHideStarted();
		
		InvokeHideStarted();
		
		_CanvasGroup.interactable   = m_Interactable;
		_CanvasGroup.blocksRaycasts = m_Interactable;
		
		yield return StartCoroutine(HideAnimationRoutine(_CanvasGroup, _Duration));
		
		_CanvasGroup.interactable   = false;
		_CanvasGroup.blocksRaycasts = false;
		
		OnHideFinished();
		
		InvokeHideFinished();
		
		if (gameObject.activeSelf)
			gameObject.SetActive(true);
	}

	protected virtual IEnumerator ShowAnimationRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return StartCoroutine(AlphaRoutine(_CanvasGroup, 1, _Duration));
	}

	protected virtual IEnumerator HideAnimationRoutine(CanvasGroup _CanvasGroup, float _Duration)
	{
		yield return StartCoroutine(AlphaRoutine(_CanvasGroup, 0, _Duration));
	}

	static IEnumerator AlphaRoutine(CanvasGroup _CanvasGroup, float _Alpha, float _Duration)
	{
		if (_CanvasGroup == null)
			yield break;
		
		float source = _CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (!Mathf.Approximately(source, target))
		{
			float time     = 0;
			float duration = _Duration * Mathf.Abs(target - source);
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_CanvasGroup.alpha = Mathf.Lerp(source, target, time / duration);
			}
		}
		
		_CanvasGroup.alpha = target;
	}
}