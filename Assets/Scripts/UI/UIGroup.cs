using System;
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

	protected float ShowDuration => m_ShowDuration;
	protected float HideDuration => m_HideDuration;

	[SerializeField] bool  m_Interactable;
	[SerializeField] float m_ShowDuration = 0.2f;
	[SerializeField] float m_HideDuration = 0.2f;

	CanvasGroup m_CanvasGroup;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		if (Shown)
			return;
		
		HideAnimation(m_HideDuration, true);
		
		CanvasGroup.interactable   = false;
		CanvasGroup.blocksRaycasts = false;
		
		gameObject.SetActive(false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
		
		Hide(true);
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
		
		if (Application.isPlaying)
			OnShowStarted();
		
		try
		{
			await ShowAnimation(m_ShowDuration, _Instant, token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		if (Application.isPlaying)
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
		
		if (Application.isPlaying)
			OnHideStarted();
		
		try
		{
			await HideAnimation(m_HideDuration, _Instant, token);
		}
		catch (TaskCanceledException)
		{
			return;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		if (Application.isPlaying)
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

	protected async Task AlphaAnimation(float _Alpha, float _Duration, bool _Instant = false, CancellationToken _Token = default)
	{
		float source = CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
		if (Mathf.Approximately(source, target))
			return;
		
		void Animation(float _Phase)
		{
			CanvasGroup.alpha = Mathf.Lerp(source, target, _Phase);
		}
		
		if (_Instant)
			Animation(1);
		else
			await UnityTask.Phase(_Phase => CanvasGroup.alpha = Mathf.Lerp(source, target, _Phase), _Duration, _Token);
	}
}