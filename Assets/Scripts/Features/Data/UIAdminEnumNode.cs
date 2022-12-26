using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIAdminEnumNode : UIAdminNode
{
	[Preserve]
	public class Pool : UIAdminNodePool { }

	[SerializeField] TMP_Text m_Value;
	[SerializeField] Button   m_IncrementButton;
	[SerializeField] Button   m_DecrementButton;

	AdminNumberNode m_Node;
	Type            m_Type;
	int[]           m_Values;

	protected override void Awake()
	{
		base.Awake();
		
		m_IncrementButton.Subscribe(Increment);
		m_DecrementButton.Subscribe(Decrement);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_IncrementButton.Unsubscribe(Increment);
		m_DecrementButton.Unsubscribe(Decrement);
	}

	public override void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		base.Setup(_Parent, _Node);
		
		m_Node = Node as AdminNumberNode;
		
		if (Node.TryGetAttribute(out AdminEnumAttribute attribute))
		{
			m_Type   = attribute.Type;
			m_Values = Enum.GetValues(attribute.Type).Cast<int>().ToArray();
		}
		
		ValueChanged();
	}

	void Increment()
	{
		if (m_Node == null || m_Values == null || m_Values.Length == 0)
			return;
		
		int data = (int)m_Node.Value;
		
		int value = (int)Enum.ToObject(m_Type, data);
		
		int index = Array.IndexOf(m_Values, value);
		
		index = MathUtility.Repeat(index + 1, m_Values.Length);
		
		m_Node.Value = m_Values[index];
	}

	void Decrement()
	{
		if (m_Node == null || m_Values == null || m_Values.Length == 0)
			return;
		
		int data = (int)m_Node.Value;
		
		int value = (int)Enum.ToObject(m_Type, data);
		
		int index = Array.IndexOf(m_Values, value);
		
		index = MathUtility.Repeat(index - 1, m_Values.Length);
		
		m_Node.Value = m_Values[index];
	}

	protected override void ValueChanged()
	{
		if (m_Node == null)
			return;
		
		m_Value.text = Enum.GetName(m_Type, (int)m_Node.Value).ToDisplayName();
	}
}
