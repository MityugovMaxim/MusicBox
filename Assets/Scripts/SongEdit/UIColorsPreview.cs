using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIColorsPreview : UIEntity
{
	[SerializeField] Button[] m_ExpandButtons;
	[SerializeField] float    m_SourceHeight = 60;
	[SerializeField] float    m_TargetHeight = 120;
	[SerializeField] float    m_Duration     = 0.2f;

	bool m_Value;

	IEnumerator m_ToggleRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		foreach (Button button in m_ExpandButtons)
			button.Subscribe(Toggle);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		foreach (Button button in m_ExpandButtons)
			button.Unsubscribe(Toggle);
	}

	void Toggle()
	{
		if (m_ToggleRoutine != null)
		{
			StopCoroutine(m_ToggleRoutine);
			m_ToggleRoutine = null;
		}
		
		m_Value = !m_Value;
		
		m_ToggleRoutine = ToggleRoutine();
		
		StartCoroutine(m_ToggleRoutine);
	}

	IEnumerator ToggleRoutine()
	{
		Vector2 size = RectTransform.sizeDelta;
		
		float source = size.y;
		float target = m_Value ? m_TargetHeight : m_SourceHeight;
		
		if (!Mathf.Approximately(source, target))
		{
			float time = 0;
			while (time < m_Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				size.y = EaseFunction.EaseOut.Get(source, target, time / m_Duration);
				
				RectTransform.sizeDelta = size;
			}
		}
		
		size.y = target;
		
		RectTransform.sizeDelta = size;
	}
}
