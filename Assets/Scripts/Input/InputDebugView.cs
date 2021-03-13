using System.Collections;
using TMPro;
using UnityEngine;

public class InputDebugView : MonoBehaviour
{
	[SerializeField] TMP_Text m_Text;

	IEnumerator m_DisplayRoutine;

	public void Display(string _Text)
	{
		if (m_DisplayRoutine != null)
			StopCoroutine(m_DisplayRoutine);
		
		m_DisplayRoutine = DisplayRoutine(_Text, 1.5f);
		
		StartCoroutine(m_DisplayRoutine);
	}

	IEnumerator DisplayRoutine(string _Text, float _Duration)
	{
		if (m_Text == null)
			yield break;
		
		m_Text.text  = _Text;
		m_Text.alpha = 1;
		
		float time     = 0;
		float duration = _Duration * 0.25f;
		
		Vector3 sourceScale = new Vector3(1.2f, 1.2f, 1.2f);
		Vector3 targetScale = new Vector3(1, 1, 1);
		
		while (time < duration)
		{
			m_Text.transform.localScale = Vector3.Lerp(sourceScale, targetScale, time / duration);
			
			yield return null;
			
			time += Time.deltaTime;
		}
		
		yield return new WaitForSeconds(_Duration * 0.5f);
		
		time = 0;
		duration = _Duration * 0.25f;
		while (time < duration)
		{
			m_Text.alpha = Mathf.Lerp(1, 0, time / duration);
			
			yield return null;
			
			time += Time.deltaTime;
		}
		
		m_Text.text  = string.Empty;
		m_Text.alpha = 1;
	}
}