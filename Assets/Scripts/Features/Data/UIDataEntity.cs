using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIDataEntity : UIEntity
{
	public string Value
	{
		get => DataNode.GetData();
		set => DataNode.SetData(value);
	}

	public bool Changed => Value != m_Default;

	static readonly Color[] m_Colors =
	{
		new Color(0f, 0.52f, 1f),
		new Color(0.52f, 0f, 1f),
		new Color(1f, 0f, 0.64f),
		new Color(1f, 0.55f, 0f),
		new Color(0f, 0.78f, 0.13f),
	};

	[SerializeField] float         m_Height;
	[SerializeField] Graphic       m_Background;
	[SerializeField] RectTransform m_Container;
	[SerializeField] TMP_Text      m_Title;
	[SerializeField] GameObject    m_Entry;
	[SerializeField] UIGroup       m_ChangedGroup;
	[SerializeField] UIButton      m_FoldoutButton;
	[SerializeField] Button        m_MoveUpButton;
	[SerializeField] Button        m_MoveDownButton;
	[SerializeField] Button        m_RemoveButton;
	[SerializeField] Button        m_RestoreButton;
	[SerializeField] Button        m_CopyButton;
	[SerializeField] Button        m_PasteButton;

	[Inject] UIDataNodeFactory m_Factory;

	IDataNodeEntry Entry => DataNode as IDataNodeEntry;

	protected DataNode DataNode { get; private set; }

	string m_Default;

	readonly List<UIDataEntity> m_Items = new List<UIDataEntity>();

	protected override void Awake()
	{
		base.Awake();
		
		m_FoldoutButton.Subscribe(Foldout);
		m_MoveUpButton.Subscribe(MoveUp);
		m_MoveDownButton.Subscribe(MoveDown);
		m_RemoveButton.Subscribe(Remove);
		m_RestoreButton.Subscribe(Restore);
		m_CopyButton.Subscribe(Copy);
		m_PasteButton.Subscribe(Paste);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_FoldoutButton.Unsubscribe(Foldout);
		m_MoveUpButton.Unsubscribe(MoveUp);
		m_MoveDownButton.Unsubscribe(MoveDown);
		m_RemoveButton.Unsubscribe(Remove);
		m_RestoreButton.Unsubscribe(Restore);
		m_CopyButton.Unsubscribe(Copy);
		m_PasteButton.Unsubscribe(Paste);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		if (DataNode != null)
			Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		if (DataNode != null)
			Unsubscribe();
	}

	public void Reposition(ref float _Position, float _Spacing)
	{
		RectTransform.anchoredPosition = new Vector2(0, -_Position);
		
		float position = 0;
		
		if (m_Container != null && m_Container.gameObject.activeSelf)
		{
			foreach (UIDataEntity item in m_Items)
				item.Reposition(ref position, _Spacing);
		}
		
		Height = position + m_Height - _Spacing;
		
		_Position += Height + _Spacing;
	}

	public void Setup(DataNode _DataNode)
	{
		if (DataNode != null)
			Unsubscribe();
		
		DataNode = _DataNode;
		
		if (m_Entry != null)
			m_Entry.SetActive(Entry != null);
		
		if (DataNode == null)
			return;
		
		ProcessData();
		
		ProcessValue();
		
		Subscribe();
	}

	void Subscribe()
	{
		DataNode.ChildAdded.AddListener(ProcessData);
		DataNode.ChildRemoved.AddListener(ProcessData);
		DataNode.ChildMoved.AddListener(ProcessData);
		DataNode.ValueChanged.AddListener(ProcessValue);
	}

	void Unsubscribe()
	{
		DataNode.ChildAdded.RemoveListener(ProcessData);
		DataNode.ChildRemoved.RemoveListener(ProcessData);
		DataNode.ChildMoved.RemoveListener(ProcessData);
		DataNode.ValueChanged.RemoveListener(ProcessValue);
	}

	void ProcessBackground()
	{
		if (m_Background == null)
			return;
		
		int index = DataNode.Level % m_Colors.Length;
		int power = DataNode.Level / m_Colors.Length;
		
		Color color = m_Colors[index];
		
		color.r *= 1.0f - power * 0.15f;
		color.g *= 1.0f - power * 0.15f;
		color.b *= 1.0f - power * 0.15f;
		
		m_Background.color = color;
	}

	protected virtual void ProcessData()
	{
		ProcessBackground();
		
		m_Title.text = DataNode.Name;
		
		m_Default = Value;
		
		if (m_Container == null)
			return;
		
		foreach (UIDataEntity item in m_Items)
			m_Factory.Remove(item);
		
		m_Items.Clear();
		
		foreach (DataNode node in DataNode)
		{
			UIDataEntity item = m_Factory.Create(node, m_Container);
			
			if (item == null)
				continue;
			
			m_Items.Add(item);
		}
		
		UIDataElement element = GetComponentInParent<UIDataElement>();
		
		if (element != null)
			element.Reposition();
	}

	protected virtual void ProcessValue()
	{
		if (m_ChangedGroup == null)
			return;
		
		if (Changed)
			m_ChangedGroup.Show();
		else
			m_ChangedGroup.Hide();
	}

	protected T GetValue<T>() => DataNode.GetValue<T>();

	protected void SetValue<T>(T _Value) => DataNode.SetValue(_Value);

	void Restore() => Value = m_Default;

	void Copy() => GUIUtility.systemCopyBuffer = Value;

	void Paste() => Value = GUIUtility.systemCopyBuffer;

	void Foldout()
	{
		if (m_Container == null)
			return;
		
		m_Container.gameObject.SetActive(!m_Container.gameObject.activeSelf);
		
		UIDataElement element = GetComponentInParent<UIDataElement>();
		
		element.Reposition();
	}

	void MoveUp() => Entry?.MoveUp();

	void MoveDown() => Entry?.MoveDown();

	void Remove() => Entry?.Remove();
}
