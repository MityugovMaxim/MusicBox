using System.Collections;
using UnityEngine;

public abstract class GotoResolver : MonoBehaviour
{
	[SerializeField] CanvasGroup m_CanvasGroup;

	bool m_Shown;

	IEnumerator m_AlphaRoutine;

	public abstract bool Resolve();

	public void Show(bool _Instant = false)
	{
		if (m_Shown)
			return;
		
		m_Shown = true;
		
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_AlphaRoutine = AlphaRoutine(1, 0.2f);
			
			StartCoroutine(m_AlphaRoutine);
		}
		else
		{
			m_CanvasGroup.alpha = 1;
		}
	}

	public void Hide(bool _Instant = false)
	{
		if (!m_Shown)
			return;
		
		m_Shown = false;
		
		if (m_AlphaRoutine != null)
			StopCoroutine(m_AlphaRoutine);
		
		if (!_Instant && gameObject.activeInHierarchy)
		{
			m_AlphaRoutine = AlphaRoutine(0, 0.2f);
			
			StartCoroutine(m_AlphaRoutine);
		}
		else
		{
			m_CanvasGroup.alpha = 0;
		}
	}

	IEnumerator AlphaRoutine(float _Alpha, float _Duration)
	{
		if (m_CanvasGroup == null)
			yield break;
		
		float source = m_CanvasGroup.alpha;
		float target = Mathf.Clamp01(_Alpha);
		
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
	}
}