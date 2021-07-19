using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class UIPause : UIEntity
{
	CanvasGroup CanvasGroup
	{
		get
		{
			if ((object)m_CanvasGroup == null)
				m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	[SerializeField] UIBlur     m_Blur;
	[SerializeField] UnityEvent m_OnShow;
	[SerializeField] UnityEvent m_OnHide;

	bool        m_Shown;
	CanvasGroup m_CanvasGroup;
	IEnumerator m_ToggleRoutine;

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (m_Shown)
				Hide();
			else
				Show();
		}
	}

	public void Toggle()
	{
		if (m_Shown)
			Hide();
		else
			Show();
	}

	public void Show()
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (m_Blur != null)
			m_Blur.Blur();
		
		if (m_ToggleRoutine != null)
			StopCoroutine(m_ToggleRoutine);
		
		m_ToggleRoutine = ShowRoutine(0.15f);
		
		StartCoroutine(m_ToggleRoutine);
	}

	public void Hide()
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (m_ToggleRoutine != null)
			StopCoroutine(m_ToggleRoutine);
		
		m_ToggleRoutine = HideRoutine(0.15f);
		
		StartCoroutine(m_ToggleRoutine);
	}

	IEnumerator ShowRoutine(float _Duration)
	{
		m_OnShow?.Invoke();
		
		CanvasGroup.interactable   = true;
		CanvasGroup.blocksRaycasts = true;
		
		yield return null;
		
		float source = CanvasGroup.alpha;
		
		const float target = 1;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				time += Time.deltaTime;
				
				CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
				
				yield return null;
			}
		}
		
		CanvasGroup.alpha = target;
	}

	IEnumerator HideRoutine(float _Duration)
	{
		yield return null;
		
		float source = CanvasGroup.alpha;
		
		const float target = 0;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < _Duration)
			{
				time += Time.deltaTime;
				
				CanvasGroup.alpha = Mathf.Lerp(source, target, time / _Duration);
				
				yield return null;
			}
		}
		
		CanvasGroup.alpha = target;
		
		CanvasGroup.interactable   = false;
		CanvasGroup.blocksRaycasts = false;
		
		m_OnHide?.Invoke();
	}
}
