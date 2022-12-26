using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIComboIndicator : UIOrder
{
	public override int Thickness => 1;

	[SerializeField] UIUnitLabel m_Label;
	[SerializeField] Graphic     m_Graphic;
	[SerializeField] Vector2     m_SourcePosition = new Vector2(0, -30);
	[SerializeField] Vector2     m_TargetPosition = Vector2.zero;
	[SerializeField] float       m_Duration       = 0.2f;

	[Inject] ScoreController m_ScoreController;

	protected override void Awake()
	{
		base.Awake();
		
		m_ScoreController.OnComboChange.AddListener(OnComboChange);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_ScoreController.OnComboChange.RemoveListener(OnComboChange);
	}

	void OnComboChange(int _Combo)
	{
		if (_Combo > m_Label.Value)
			Increment();
		else
			Decrement();
		
		m_Label.Value = _Combo;
	}

	IEnumerator m_ComboRoutine;

	void Increment()
	{
		if (m_ComboRoutine != null)
			StopCoroutine(m_ComboRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_ComboRoutine = UnityRoutine.PositionRoutine(m_Graphic.rectTransform, m_SourcePosition, m_TargetPosition, m_Duration, EaseFunction.EaseOutBack);
		
		StartCoroutine(m_ComboRoutine);
	}

	void Decrement()
	{
		if (m_ComboRoutine != null)
			StopCoroutine(m_ComboRoutine);
		
		if (!gameObject.activeInHierarchy)
			return;
		
		m_ComboRoutine = UnityRoutine.PositionRoutine(m_Graphic.rectTransform, m_TargetPosition, m_Duration, EaseFunction.EaseOut);
		
		StartCoroutine(m_ComboRoutine);
	}
}
