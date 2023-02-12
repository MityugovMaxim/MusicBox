using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public abstract class UIAdminNode : UIEntity
{
	public RectTransform Container => m_Container;

	protected AdminNode Node { get; private set; }

	protected virtual IReadOnlyList<AdminNode> Children => Node.Children;

	[SerializeField] Graphic       m_Background;
	[SerializeField] Graphic[]     m_Changed;
	[SerializeField] UIAdminField  m_NameField;
	[SerializeField] TMP_Text      m_NameLabel;
	[SerializeField] RectOffset    m_Margin;
	[SerializeField] RectTransform m_Container;
	[SerializeField] UIGroup       m_ChangedGroup;
	[SerializeField] Button        m_ExpandButton;
	[SerializeField] Button        m_CollapseButton;
	[SerializeField] Button        m_CopyButton;
	[SerializeField] Button        m_PasteButton;
	[SerializeField] Button        m_RemoveButton;
	[SerializeField] Button        m_SelectButton;
	[SerializeField] Button        m_InsertButton;
	[SerializeField] Button        m_RestoreButton;

	[Inject] UIAdminNodeFactory m_Factory;
	[Inject] MenuProcessor      m_MenuProcessor;

	readonly List<UIAdminNode> m_Items = new List<UIAdminNode>();

	UIAdminNode m_Parent;
	bool        m_DirtyPosition = true;
	float       m_MinPosition;
	float       m_MaxPosition;

	protected override void Awake()
	{
		base.Awake();
		
		m_NameField.Subscribe(Rename);
		m_ExpandButton.Subscribe(Expand);
		m_CollapseButton.Subscribe(Collapse);
		m_CopyButton.Subscribe(Copy);
		m_PasteButton.Subscribe(Paste);
		m_SelectButton.Subscribe(Select);
		m_InsertButton.Subscribe(Insert);
		m_RemoveButton.Subscribe(Remove);
		m_RestoreButton.Subscribe(Restore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_NameField.Unsubscribe(Rename);
		m_ExpandButton.Unsubscribe(Expand);
		m_CollapseButton.Unsubscribe(Collapse);
		m_CopyButton.Unsubscribe(Copy);
		m_PasteButton.Unsubscribe(Paste);
		m_SelectButton.Unsubscribe(Select);
		m_InsertButton.Unsubscribe(Insert);
		m_RemoveButton.Unsubscribe(Remove);
		m_RestoreButton.Unsubscribe(Restore);
	}

	public virtual void Setup(UIAdminNode _Parent, AdminNode _Node)
	{
		if (Node != null)
		{
			Node.Unsubscribe(ValueChanged);
			Node.Unsubscribe(ProcessState);
		}
		
		RemoveNodes();
		
		m_Parent = _Parent;
		Node     = _Node;
		
		if (Node != null)
		{
			Node.Subscribe(ValueChanged);
			Node.Subscribe(ProcessState);
		}
		
		ProcessBackground();
		
		ProcessName();
		
		ProcessControl();
		
		ProcessState();
		
		if (Node != null && Node.Expanded)
			Expand();
		else
			Collapse();
		
		SetDirtyPosition();
	}

	protected abstract void ValueChanged();

	void ProcessBackground()
	{
		if (m_Background == null)
			return;
		
		Color color = new Color(0.15f, 0.35f, 0.5f);
		
		Color.RGBToHSV(color, out float h, out float s, out float v);
		
		int indent = Node.Indent;
		
		h = Mathf.Repeat(h - indent * 0.1f, 1);
		
		m_Background.color = Color.HSVToRGB(h, s, v);
		
		color = new Color(0, 0.55f, 0.7f);
		
		Color.RGBToHSV(color, out h, out s, out v);
		
		h = Mathf.Repeat(h + 0.33333f, 1);
		
		color = Color.HSVToRGB(h, s, v);
		
		foreach (Graphic item in m_Changed)
			item.color = color;
	}

	void ProcessName()
	{
		if (Node.TryGetAttribute(out AdminTitleAttribute attribute))
		{
			m_NameField.gameObject.SetActive(false);
			m_NameLabel.gameObject.SetActive(true);
			m_NameLabel.text = attribute.Title;
		}
		else if (Node.HasAttribute<AdminFixedAttribute>())
		{
			m_NameField.gameObject.SetActive(false);
			m_NameLabel.gameObject.SetActive(true);
			m_NameLabel.text = Node.Name.ToDisplayName();
		}
		else
		{
			m_NameLabel.gameObject.SetActive(false);
			m_NameField.gameObject.SetActive(true);
			m_NameField.Value = Node.Name;
		}
		
		if (Node.HasAttribute<AdminUpperAttribute>())
		{
			m_NameField.Value = m_NameField.Value.ToUpperInvariant();
			m_NameLabel.text  = m_NameLabel.text.ToUpperInvariant();
		}
	}

	void ProcessControl()
	{
		if (m_InsertButton != null)
			m_InsertButton.gameObject.SetActive(Node.HasAttribute<AdminCollectionAttribute>());
		if (m_RemoveButton != null)
			m_RemoveButton.gameObject.SetActive(m_Parent != null && !Node.HasAttribute<AdminTitleAttribute>() && !Node.HasAttribute<AdminFixedAttribute>());
		if (m_SelectButton != null)
			m_SelectButton.gameObject.SetActive(m_Parent != null);
	}

	void Rename(string _Name)
	{
		if (string.IsNullOrEmpty(_Name))
			return;
		
		Node.Rename(_Name);
		
		ProcessName();
	}

	void Insert()
	{
		Expand();
		
		Node.Insert();
	}

	void Remove()
	{
		Collapse();
		
		Node.Remove();
	}

	void Restore() => Node.Restore();

	void Expand()
	{
		if (Container == null)
			return;
		
		Node.Expanded = true;
		
		Container.gameObject.SetActive(true);
		m_CollapseButton.gameObject.SetActive(true);
		m_ExpandButton.gameObject.SetActive(false);
		
		CreateNodes();
		
		SetDirtyPosition();
	}

	void Collapse()
	{
		if (Container == null)
			return;
		
		Node.Expanded = false;
		
		Container.gameObject.SetActive(false);
		m_CollapseButton.gameObject.SetActive(false);
		m_ExpandButton.gameObject.SetActive(true);
		
		RemoveNodes();
		
		SetDirtyPosition();
	}

	void Copy() => GUIUtility.systemCopyBuffer = Node.Copy();

	void Paste() => Node.Paste(GUIUtility.systemCopyBuffer);

	void Select()
	{
		Expand();
		
		UIAdminMenu adminMenu = m_MenuProcessor.GetMenu<UIAdminMenu>();
		
		if (adminMenu == null)
			return;
		
		adminMenu.Select(Node);
	}

	void SetDirtyPosition()
	{
		m_DirtyPosition = true;
		if (m_Parent != null)
			m_Parent.SetDirtyPosition();
	}

	void LateUpdate()
	{
		if (m_DirtyPosition && m_Parent == null)
			Reposition(0);
	}

	float Reposition(float _Position)
	{
		const float spacing = 10;
		
		if (!m_DirtyPosition)
		{
			float height = m_MaxPosition - m_MinPosition;
			m_MinPosition = _Position;
			m_MaxPosition = _Position + height;
			
			ProcessPosition();
			
			return m_MaxPosition;
		}
		
		m_DirtyPosition = false;
		m_MinPosition   = _Position;
		m_MaxPosition   = _Position;
		
		float position = 0;
		if (Container != null && Container.gameObject.activeSelf)
		{
			foreach (UIAdminNode item in m_Items)
				position = item.Reposition(position) + spacing;
			position = Mathf.Max(0, position - spacing);
		}
		
		m_MaxPosition = m_MinPosition + position + m_Margin.vertical;
		
		ProcessPosition();
		
		return m_MaxPosition;
	}

	void ProcessPosition()
	{
		if (m_Parent != null)
			Y = -m_MinPosition;
		
		Height = m_MaxPosition - m_MinPosition;
	}

	void ProcessState()
	{
		if (m_Parent != null)
			m_Parent.ProcessState();
		
		if (m_ChangedGroup == null)
			return;
		
		if (Node != null && Node.Changed)
			m_ChangedGroup.Show();
		else
			m_ChangedGroup.Hide();
	}

	void CreateNodes()
	{
		if (Container == null || !Container.gameObject.activeSelf)
			return;
		
		foreach (AdminNode node in Children)
		{
			if (!FilterNode(node))
				continue;
			
			UIAdminNode item = m_Factory.Spawn(this, node);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
	}

	protected virtual bool FilterNode(AdminNode _Node) => true;

	protected void RefreshNodes()
	{
		RemoveNodes();
		
		CreateNodes();
		
		SetDirtyPosition();
	}

	void RemoveNodes()
	{
		foreach (UIAdminNode item in m_Items)
			m_Factory.Despawn(item);
		m_Items.Clear();
	}
}
