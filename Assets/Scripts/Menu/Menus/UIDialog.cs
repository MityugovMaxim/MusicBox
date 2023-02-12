using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : UIMenu
{
	[SerializeField] Button m_CancelButton;
	[SerializeField] Button m_BackgroundButton;

	IEnumerator m_ToggleRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		m_CancelButton.Subscribe(Cancel);
		m_BackgroundButton.Subscribe(Cancel);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CancelButton.Unsubscribe(Cancel);
		m_BackgroundButton.Unsubscribe(Cancel);
	}

	public override void OnFocusGain()
	{
		Toggle(true);
	}

	public override void OnFocusLose()
	{
		Toggle(false);
	}

	void Cancel() => Hide();

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		StopToggle();
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		
		StopToggle();
	}

	void StopToggle()
	{
		if (m_ToggleRoutine == null)
			return;
		
		StopCoroutine(m_ToggleRoutine);
		
		m_ToggleRoutine = null;
	}

	void Toggle(bool _Value)
	{
		StopToggle();
		
		if (gameObject.activeInHierarchy)
		{
			m_ToggleRoutine = ToggleRoutine(_Value);
			
			StartCoroutine(m_ToggleRoutine);
		}
		else
		{
			CanvasGroup.alpha = _Value ? 1 : 0;
		}
	}

	IEnumerator ToggleRoutine(bool _Value)
	{
		float source   = CanvasGroup.alpha;
		float target   = _Value ? 1 : 0;
		float duration = _Value ? ShowDuration : HideDuration;
		if (!Mathf.Approximately(source, target) && duration > float.Epsilon)
		{
			float time = 0;
			while (time < duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				CanvasGroup.alpha = Mathf.Lerp(source, target, time / duration);
			}
		}
		CanvasGroup.alpha = target;
	}
}
