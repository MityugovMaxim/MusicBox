using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public partial class HoldCurve
{
	public void Reposition()
	{
		m_Keys.Sort((_A, _B) => _A.Time.CompareTo(_B.Time));
		
		for (int i = 1; i < m_Keys.Count; i++)
		{
			var source = m_Keys[i - 1];
			var target = m_Keys[i];
			
			float distance = target.Time - source.Time;
			
			source.OutTangent = new Vector2(distance * 0.3f, 0);
			target.InTangent  = new Vector2(-distance * 0.3f, 0);
		}
	}
}
#endif

[Serializable]
public partial class HoldCurve : IEnumerable<HoldCurve.Key>
{
	[Serializable]
	public class Key
	{
		public float   Time
		{
			get => m_Time;
			set => m_Time = value;
		}

		public float   Value
		{
			get => m_Value;
			set => m_Value = value;
		}

		public Vector2 InTangent
		{
			get => m_InTangent;
			set => m_InTangent = value;
		}

		public Vector2 OutTangent
		{
			get => m_OutTangent;
			set => m_OutTangent = value;
		}

		[SerializeField] float   m_Time;
		[SerializeField] float   m_Value;
		[SerializeField] Vector2 m_InTangent;
		[SerializeField] Vector2 m_OutTangent;

		public Key(float _Time, float _Value, Vector2 _InTangent, Vector2 _OutTangent)
		{
			m_Time       = _Time;
			m_Value      = _Value;
			m_InTangent  = _InTangent;
			m_OutTangent = _OutTangent;
		}
	}

	public int Length => m_Keys?.Count ?? 0;

	public Key this[int _Index] => m_Keys?[_Index];

	[SerializeField] List<Key> m_Keys = new List<Key>();

	public void Add(Key _Key)
	{
		m_Keys.Add(_Key);
	}

	public void Remove(Key _Key)
	{
		m_Keys.Remove(_Key);
	}

	public IEnumerator<Key> GetEnumerator()
	{
		return m_Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
