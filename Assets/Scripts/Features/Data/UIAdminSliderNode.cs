using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdminSliderNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	[SerializeField] Slider   m_Slider;
	[SerializeField] TMP_Text m_Value;

	AdminNumberNode m_Node;
	float           m_Min;
	float           m_Max;
	int             m_Steps;

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

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminNumberNode;
		
		if (Node.TryGetAttribute(out AdminSliderAttribute attribute))
		{
			m_Min   = attribute.Min;
			m_Max   = attribute.Max;
			m_Steps = attribute.Steps;
		}
		else
		{
			m_Min   = 0;
			m_Max   = 1;
			m_Steps = 0;
		}
		
		m_Slider.minValue     = 0;
		m_Slider.maxValue     = m_Steps > 1 ? m_Steps - 1 : 1;
		m_Slider.wholeNumbers = m_Steps > 0;
		
		ValueChanged();
	}

	void ProcessValue(float _Value)
	{
		if (m_Node == null)
			return;
		
		float value = MathUtility.RemapClamped(_Value, m_Slider.minValue, m_Slider.maxValue, m_Min, m_Max);
		
		m_Node.Value = (decimal)value;
	}

	protected override void ValueChanged()
	{
		if (m_Node == null)
			return;
		
		decimal value = m_Node.Value;
		
		m_Slider.value = MathUtility.RemapClamped((float)value, m_Min, m_Max, m_Slider.minValue, m_Slider.maxValue);
		m_Value.text   = value.ToString("0.###", CultureInfo.InvariantCulture);
	}
}
