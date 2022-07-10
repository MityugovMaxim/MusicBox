using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIOrder : UIEntity
{
	public virtual int Thickness => m_Thickness;

	public enum OrderMode
	{
		Follow,
		Parallel,
		Manual
	}

	bool Root => GetParent() == null;

	UIOrder m_Parent;

	readonly List<UIOrder> m_Children = new List<UIOrder>();

	[SerializeField] OrderMode m_Mode;
	[SerializeField] int       m_Thickness;

	[NonSerialized] bool m_DirtyOrder    = true;
	[NonSerialized] bool m_DirtyParent   = true;
	[NonSerialized] bool m_DirtyChildren = true;

	int  m_Depth;

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		SetDirtyOrder();
		
		EditorApplication.QueuePlayerLoopUpdate();
	}
	#endif

	void LateUpdate()
	{
		if (m_DirtyOrder && Root)
			Reorder();
	}

	void OnTransformChildrenChanged()
	{
		if (m_Mode == OrderMode.Manual)
			return;
		
		SetDirtyChildren();
		
		SetDirtyOrder();
	}

	protected override void OnBeforeTransformParentChanged()
	{
		base.OnBeforeTransformParentChanged();
		
		SetDirtyOrder();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		
		SetDirtyParent();
		
		SetDirtyOrder();
	}

	int Reorder()
	{
		if (!m_DirtyOrder)
			return m_Depth;
		
		m_DirtyOrder = false;
		
		m_Depth = Thickness;
		
		if (m_Mode == OrderMode.Follow)
		{
			foreach (UIOrder child in GetChildren())
			{
				child.SetPosition(m_Depth);
				
				m_Depth += child.Reorder();
			}
		}
		else if (m_Mode == OrderMode.Parallel)
		{
			int depth = m_Depth;
			foreach (UIOrder child in GetChildren())
			{
				child.SetPosition(depth);
				
				m_Depth = Mathf.Max(m_Depth, child.Reorder());
			}
		}
		
		return m_Depth;
	}

	void SetPosition(int _Position)
	{
		Vector3 position = RectTransform.localPosition;
		
		position.z = -_Position;
		
		RectTransform.localPosition = position;
	}

	void SetDirtyParent()
	{
		m_DirtyParent = true;
	}

	void SetDirtyChildren()
	{
		m_DirtyChildren = true;
	}

	void SetDirtyOrder()
	{
		m_DirtyOrder = true;
		
		UIOrder parent = GetParent();
		
		if (parent != null)
			parent.SetDirtyOrder();
	}

	UIOrder GetParent()
	{
		if (m_DirtyParent)
		{
			m_DirtyParent = false;
			
			Transform parent = RectTransform.parent;
			
			m_Parent = parent != null ? parent.GetComponent<UIOrder>() : null;
		}
		
		return m_Parent;
	}

	List<UIOrder> GetChildren()
	{
		if (m_DirtyChildren)
		{
			m_DirtyChildren = false;
			m_Children.Clear();
			for (int i = 0; i < RectTransform.childCount; i++)
			{
				UIOrder child = RectTransform.GetChild(i).GetComponent<UIOrder>();
				if (child != null)
					m_Children.Add(child);
			}
		}
		return m_Children;
	}
}