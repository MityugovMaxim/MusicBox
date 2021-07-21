using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class UIMenu : UIEntity
{
	[SerializeField] UIBlur     m_Blur;
	[SerializeField] UnityEvent m_OnShowStarted;
	[SerializeField] UnityEvent m_OnShowFinished;
	[SerializeField] UnityEvent m_OnHideStarted;
	[SerializeField] UnityEvent m_OnHideFinished;

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

	public void Toggle(bool _Instant = false, UnityAction _Started = null, UnityAction _Finished = null)
	{
		if (m_Shown)
			Hide(_Instant, _Started, _Finished);
		else
			Show(_Instant, _Started, _Finished);
	}

	public void Show(bool _Instant = false, UnityAction _Started = null, UnityAction _Finished = null)
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (_Started != null)
			m_OnShowStarted.AddListener(_Started);
		
		if (_Finished != null)
			m_OnShowFinished.AddListener(_Finished);
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_CanvasGroup.alpha          = 1;
			m_CanvasGroup.interactable   = true;
			m_CanvasGroup.blocksRaycasts = true;
			
			OnShowFinished();
			
			if (m_OnShowFinished != null)
			{
				m_OnShowFinished.Invoke();
				m_OnShowFinished.RemoveAllListeners();
			}
		}
		else
		{
			m_Routine = ShowRoutine(0.2f);
			
			StartCoroutine(m_Routine);
		}
	}

	public void Hide(bool _Instant = false, UnityAction _Started = null, UnityAction _Finished = null)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (_Started != null)
			m_OnHideStarted.AddListener(_Started);
		
		if (_Finished != null)
			m_OnHideFinished.AddListener(_Finished);
		
		if (m_Routine != null)
			StopCoroutine(m_Routine);
		
		if (_Instant || !gameObject.activeInHierarchy)
		{
			m_CanvasGroup.alpha          = 0;
			m_CanvasGroup.interactable   = false;
			m_CanvasGroup.blocksRaycasts = false;
			
			OnHideFinished();
			
			if (m_OnHideFinished != null)
			{
				m_OnHideFinished.Invoke();
				m_OnHideFinished.RemoveAllListeners();
			}
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

	IEnumerator ShowRoutine(float _Duration)
	{
		m_CanvasGroup.interactable   = true;
		m_CanvasGroup.blocksRaycasts = true;
		
		OnShowStarted();
		
		if (m_OnShowStarted != null)
		{
			m_OnShowStarted.Invoke();
			m_OnShowStarted.RemoveAllListeners();
		}
		
		if (m_Blur != null)
			m_Blur.Blur();
		
		yield return null;
		
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
		
		if (m_OnShowFinished != null)
		{
			m_OnShowFinished.Invoke();
			m_OnShowFinished.RemoveAllListeners();
		}
	}

	IEnumerator HideRoutine(float _Duration)
	{
		OnHideStarted();
		
		if (m_OnHideStarted != null)
		{
			m_OnHideStarted.Invoke();
			m_OnHideStarted.RemoveAllListeners();
		}
		
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
		
		if (m_OnHideFinished != null)
		{
			m_OnHideFinished.Invoke();
			m_OnHideFinished.RemoveAllListeners();
		}
	}
}