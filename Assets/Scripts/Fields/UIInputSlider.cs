using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIInputSlider<T> : UIField<T>
{
	public event Action<T> OnSubmit;

	[SerializeField] TMP_Text m_Label;
	[SerializeField] TMP_Text m_Value;
	[SerializeField] Slider   m_Slider;
	[SerializeField] UIGroup  m_Changed;

	protected override void Awake()
	{
		base.Awake();
		
		m_Slider.onValueChanged.AddListener(Submit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Slider.onValueChanged.RemoveListener(Submit);
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		base.Reset();
		
		m_Label  = Transform.Find("label").GetComponent<TMP_Text>();
		m_Slider = Transform.Find("slider").GetComponent<Slider>();
	}
	#endif

	protected override void Refresh()
	{
		m_Label.text   = Name;
		m_Slider.value = ParseFloat(Value);
		
		if (m_Changed == null)
			return;
		
		if (Changed)
			m_Changed.Show();
		else
			m_Changed.Hide();
	}

	void Submit(float _Value)
	{
		Value = ParseValue(_Value);
		
		m_Value.text = Value.ToString();
		
		Refresh();
		
		OnSubmit?.Invoke(Value);
	}

	protected abstract float ParseFloat(T _Value);

	protected abstract T ParseValue(float _Value);
}