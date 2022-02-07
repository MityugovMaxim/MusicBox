using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIGroup : UIEntity
{
	public bool Shown { get; private set; }

	protected CanvasGroup CanvasGroup
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

	IEnumerator m_AlphaRoutine;

	Action m_AlphaFinished;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		if (Shown)
			return;
		
		CanvasGroup.alpha          = 0;
		CanvasGroup.interactable   = false;
		CanvasGroup.blocksRaycasts = false;
		gameObject.SetActive(false);
	}

	public async Task ShowAsync(bool _Instant = false)
	{
		if (Shown)
			return;
		
		Shown = true;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		gameObject.SetActive(true);
		
		CanvasGroup.interactable   = m_Interactable;
		CanvasGroup.blocksRaycasts = m_Interactable;
		
		OnShowStarted();
		
		await ShowAnimation(m_ShowDuration, _Instant, token);
		
		OnShowFinished();
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public async Task HideAsync(bool _Instant = false)
	{
		if (!Shown)
			return;
		
		Shown = false;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		OnHideStarted();
		
		await HideAnimation(m_HideDuration, _Instant, token);
		
		OnHideFinished();
		
		CanvasGroup.interactable   = false;
		CanvasGroup.blocksRaycasts = false;
		
		gameObject.SetActive(false);
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public async void Show(bool _Instant = false, Action _Finished = null)
	{
		await ShowAsync(_Instant);
		
		_Finished?.Invoke();
	}

	public async void Hide(bool _Instant = false, Action _Finished = null)
	{
		await HideAsync(_Instant);
		
		_Finished?.Invoke();
	}

	protected virtual void OnShowStarted() { }

	protected virtual void OnShowFinished() { }

	protected virtual void OnHideStarted() { }

	protected virtual void OnHideFinished() { }

	protected virtual Task ShowAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return AlphaAnimation(1, _Duration, _Instant, _Token);
	}

	protected virtual Task HideAnimation(float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		return AlphaAnimation(0, _Duration, _Instant, _Token);
	}

	protected Task AlphaAnimation(float _Alpha, float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		InvokeAlphaFinished();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_AlphaFinished = () => completionSource.TrySetResult(true);
		
		if (_Token.IsCancellationRequested)
		{
			InvokeAlphaFinished();
			return completionSource.Task;
		}
		
		_Token.Register(
			() =>
			{
				if (m_AlphaRoutine != null)
					StopCoroutine(m_AlphaRoutine);
				
				InvokeAlphaFinished();
			}
		);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_AlphaRoutine = AlphaRoutine(CanvasGroup, _Alpha, _Duration, InvokeAlphaFinished);
			
			StartCoroutine(m_AlphaRoutine);
		}
		else
		{
			CanvasGroup.alpha = _Alpha;
			
			InvokeAlphaFinished();
		}
		
		return completionSource.Task;
	}

	static IEnumerator AlphaRoutine(CanvasGroup _CanvasGroup, float _Alpha, float _Duration, Action _Finished)
	{
		if (_CanvasGroup == null)
		{
			_Finished?.Invoke();
			yield break;
		}
		
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
		
		_Finished?.Invoke();
	}

	void InvokeAlphaFinished()
	{
		Action action = m_AlphaFinished;
		m_AlphaFinished = null;
		action?.Invoke();
	}
}