using System;
using UnityEngine;

[Serializable]
public struct Range
{
	public float Min
	{
		get => m_Min;
		set => m_Min = value;
	}

	public float Max
	{
		get => m_Max;
		set => m_Max = value;
	}

	[SerializeField] float m_Min;
	[SerializeField] float m_Max;

	public Range(float _Min, float _Max)
	{
		m_Min = _Min;
		m_Max = _Max;
	}

	public bool Contains(float _Value)
	{
		return m_Min <= _Value && m_Max >= _Value;
	}
}