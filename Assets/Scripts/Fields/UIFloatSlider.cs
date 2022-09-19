using UnityEngine;

public class UIFloatSlider : UIInputSlider<float>
{
	[SerializeField] float m_MinValue;
	[SerializeField] float m_MaxValue;

	public override void SetAttribute<TAttribute>(TAttribute _Attribute)
	{
		RangeAttribute rangeAttribute = _Attribute as RangeAttribute;
		
		if (rangeAttribute == null)
			return;
		
		m_MinValue = rangeAttribute.min;
		m_MaxValue = rangeAttribute.max;
	}

	protected override float ParseFloat(float _Value)
	{
		return Mathf.InverseLerp(m_MinValue, m_MaxValue, _Value);
	}

	protected override float ParseValue(float _Value)
	{
		return Mathf.Lerp(m_MinValue, m_MaxValue, _Value);
	}
}