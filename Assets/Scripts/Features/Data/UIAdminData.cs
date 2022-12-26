using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIAdminNodeFactory
{
	[Inject] UIAdminNumberNode.Pool  m_NumberPool;
	[Inject] UIAdminStringNode.Pool  m_StringPool;
	[Inject] UIAdminBooleanNode.Pool m_BooleanPool;
	[Inject] UIAdminObjectNode.Pool  m_ObjectPool;
	[Inject] UIAdminArrayNode.Pool   m_ArrayPool;
	[Inject] UIAdminSliderNode.Pool  m_SliderPool;
	[Inject] UIAdminDateNode.Pool    m_DatePool;
	[Inject] UIAdminTimeNode.Pool    m_TimePool;
	[Inject] UIAdminTickNode.Pool    m_TickPool;
	[Inject] UIAdminEnumNode.Pool    m_EnumPool;
	[Inject] UIAdminSearchNode.Pool  m_SearchPool;

	// [Inject] UIAdminAreaNode.Pool    m_AreaPool;

	readonly Dictionary<UIAdminNode, UIAdminNodePool> m_Instances = new Dictionary<UIAdminNode, UIAdminNodePool>();

	UIAdminNodePool GetPool(UIAdminNode _Node)
	{
		if (_Node != null && m_Instances.TryGetValue(_Node, out UIAdminNodePool pool))
			return pool;
		return null;
	}

	UIAdminNodePool GetPool(AdminNode _Node)
	{
		if (_Node == null)
			return null;
		
		if (_Node.HasAttribute<AdminHideAttribute>())
			return null;
		
		switch (_Node.Type)
		{
			case AdminNodeType.Number:
				if (_Node.HasAttribute<AdminSliderAttribute>())
					return m_SliderPool;
				if (_Node.HasAttribute<AdminDateAttribute>())
					return m_DatePool;
				if (_Node.HasAttribute<AdminTimeAttribute>())
					return m_TimePool;
				if (_Node.HasAttribute<AdminTickAttribute>())
					return m_TickPool;
				if (_Node.HasAttribute<AdminEnumAttribute>())
					return m_EnumPool;
				return m_NumberPool;
			case AdminNodeType.String:
				return m_StringPool;
			case AdminNodeType.Boolean:
				return m_BooleanPool;
			case AdminNodeType.Object:
				if (_Node.HasAttribute<AdminSearchAttribute>())
					return m_SearchPool;
				return m_ObjectPool;
			case AdminNodeType.Array:
				return m_ArrayPool;
			default:
				return null;
		}
	}

	public UIAdminNode Spawn(RectTransform _Container, AdminNode _Node)
	{
		UIAdminNode node = Spawn(default(UIAdminNode), _Node);
		
		if (_Container != null)
			node.RectTransform.SetParent(_Container, false);
		
		return node;
	}

	public UIAdminNode Spawn(UIEntity _Container, AdminNode _Node)
	{
		UIAdminNode node = Spawn(default(UIAdminNode), _Node);
		
		if (_Container != null)
			node.RectTransform.SetParent(_Container.RectTransform, false);
		
		return node;
	}

	public UIAdminNode Spawn(UIAdminNode _Parent, AdminNode _Node)
	{
		UIAdminNodePool pool = GetPool(_Node);
		
		if (pool == null)
			return null;
		
		UIAdminNode node = pool.Spawn(_Node, _Parent);
		
		if (node == null)
			return null;
		
		m_Instances[node] = pool;
		
		return node;
	}

	public void Despawn(UIAdminNode _Node)
	{
		UIAdminNodePool pool = GetPool(_Node);
		
		if (pool == null)
			return;
		
		pool.Despawn(_Node);
		
		m_Instances.Remove(_Node);
	}
}

public class UIAdminNodePool : MonoMemoryPool<AdminNode, UIAdminNode, UIAdminNode>
{
	protected override void Reinitialize(AdminNode _Node, UIAdminNode _Parent, UIAdminNode _Item)
	{
		if (_Parent != null)
			_Item.RectTransform.SetParent(_Parent.Container, false);
		
		_Item.Setup(_Parent, _Node);
	}
}

public class UIAdminAreaNode : UIAdminNode
{
	public class Pool : UIAdminNodePool { }

	protected override void ValueChanged()
	{
		
	}
}
