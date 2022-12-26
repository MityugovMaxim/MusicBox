using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIDataSlider : UIDataEntity
{
	[Preserve]
	public class Pool : UIDataEntityPool<UIDataSlider> { }

	[SerializeField] Slider   m_Slider;
	[SerializeField] TMP_Text m_Min;
	[SerializeField] TMP_Text m_Max;

	protected override void Awake()
	{
		base.Awake();
		
		m_Slider.Subscribe(ProcessValue);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Slider.Unsubscribe(ProcessValue);
	}

	protected override void ProcessData()
	{
		base.ProcessData();
		
		if (!DataNode.TryGetAttribute(out DataSliderAttribute attribute))
			return;
		
		m_Min.text        = attribute.Min.ToString(CultureInfo.InvariantCulture);
		m_Max.text        = attribute.Max.ToString(CultureInfo.InvariantCulture);
		m_Slider.minValue = attribute.Min;
		m_Slider.maxValue = attribute.Max;
		m_Slider.SetValueWithoutNotify(GetValue<float>());
	}

	protected override void ProcessValue()
	{
		m_Slider.SetValueWithoutNotify(GetValue<float>());
	}

	void ProcessValue(float _Value)
	{
		SetValue(_Value);
	}
}
