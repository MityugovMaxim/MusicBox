using TMPro;
using UnityEngine;

public class UICascadeUnitLabel : UICascadeLabel
{
	public double Value
	{
		get => m_Value;
		set
		{
			if (m_Value.Equals(value))
				return;
			
			m_Value = value;
			
			ProcessValue();
		}
	}

	protected override TMP_Text this[int _Index] => m_Labels[_Index];

	protected override int Count => m_Labels.Length;

	[SerializeField] double        m_Value;
	[SerializeField] UIUnitLabel[] m_Labels;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessValue();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessValue();
	}
	#endif

	void ProcessValue()
	{
		foreach (UIUnitLabel label in m_Labels)
			label.Value = m_Value;
	}
}