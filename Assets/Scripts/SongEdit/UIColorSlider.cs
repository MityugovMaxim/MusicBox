using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIColorSlider : UIEntity
{
	[Serializable]
	public class ValueChanged : UnityEvent<int> { }

	public ValueChanged OnValueChanged => m_OnValueChanged;

	public int Value
	{
		get => Mathf.RoundToInt(m_Slider.value);
		set => m_Slider.value = Mathf.Clamp(value, 0, 255);
	}

	[SerializeField] Slider         m_Slider;
	[SerializeField] TMP_InputField m_Field;
	[SerializeField] ValueChanged   m_OnValueChanged;

	protected override void Awake()
	{
		base.Awake();
		
		m_Slider.onValueChanged.AddListener(ProcessSliderValue);
		m_Field.onValueChanged.AddListener(ProcessFieldValue);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Slider.onValueChanged.RemoveAllListeners();
		m_Field.onValueChanged.RemoveAllListeners();
	}

	void ProcessSliderValue(float _Value)
	{
		int value = Mathf.RoundToInt(_Value);
		
		value = Mathf.Clamp(value, 0, 255);
		
		m_Field.SetTextWithoutNotify(value.ToString());
		
		m_OnValueChanged?.Invoke(value);
	}

	void ProcessFieldValue(string _Text)
	{
		int.TryParse(_Text, out int value);
		
		value = Mathf.Clamp(value, 0, 255);
		
		m_Slider.SetValueWithoutNotify(value);
		
		m_OnValueChanged?.Invoke(value);
	}
}