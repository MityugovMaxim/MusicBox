using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIComboIndicator : UIOrder
{
	public override int Thickness => 1;

	[SerializeField] UIUnitLabel m_Label;
	[SerializeField] Graphic     m_Graphic;
	[SerializeField] Color       m_DefaultColor;
	[SerializeField] Color       m_PerfectColor;
	[SerializeField] Color       m_GoodColor;
	[SerializeField] Color       m_BadColor;
	[SerializeField] Color       m_FailColor;
	[SerializeField] Vector2     m_SourcePosition = new Vector2(0, -30);
	[SerializeField] Vector2     m_TargetPosition = Vector2.zero;
	[SerializeField] float       m_Duration       = 0.2f;

	[Inject] ScoreManager m_ScoreManager;

	protected override void Awake()
	{
		base.Awake();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnComboChanged += OnComboChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		if (m_ScoreManager != null)
			m_ScoreManager.OnComboChanged -= OnComboChanged;
	}

	void OnComboChanged(int _Combo, ScoreGrade _Grade)
	{
		Color color;
		switch (_Grade)
		{
			case ScoreGrade.Perfect:
				color = m_PerfectColor;
				break;
			case ScoreGrade.Good:
				color = m_GoodColor;
				break;
			case ScoreGrade.Bad:
				color = m_BadColor;
				break;
			case ScoreGrade.Fail:
				color = m_FailColor;
				break;
			default:
				color = m_DefaultColor;
				break;
		}
		
		if (m_Label.Value < _Combo)
			Increment(color);
		else
			Decrement(color);
		
		m_Label.Value = _Combo;
	}

	IEnumerator m_ColorRoutine;
	IEnumerator m_PositionRoutine;

	void Increment(Color _Color)
	{
		if (m_ColorRoutine != null)
			StopCoroutine(m_ColorRoutine);
		
		if (m_PositionRoutine != null)
			StopCoroutine(m_PositionRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_ColorRoutine = UnityRoutine.ColorRoutine(m_Graphic, _Color, m_DefaultColor, m_Duration, EaseFunction.Linear);
		
		m_PositionRoutine = UnityRoutine.PositionRoutine(m_Graphic.rectTransform, m_SourcePosition, m_TargetPosition, m_Duration, EaseFunction.EaseOutBack);
		
		StartCoroutine(m_ColorRoutine);
		StartCoroutine(m_PositionRoutine);
	}

	void Decrement(Color _Color)
	{
		if (m_ColorRoutine != null)
			StopCoroutine(m_ColorRoutine);
		
		if (m_PositionRoutine != null)
			StopCoroutine(m_PositionRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_ColorRoutine = UnityRoutine.ColorRoutine(m_Graphic, _Color, m_Duration, EaseFunction.Linear);
		
		m_PositionRoutine = UnityRoutine.PositionRoutine(m_Graphic.rectTransform, m_TargetPosition, m_Duration, EaseFunction.EaseOut);
		
		StartCoroutine(m_ColorRoutine);
		StartCoroutine(m_PositionRoutine);
	}
}