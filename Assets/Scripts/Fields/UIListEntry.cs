using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public abstract class UIListEntry : UIEntity
{
	[Preserve]
	public class Factory : PlaceholderFactory<IListField, int, RectTransform, UIListEntry> { }

	protected int Index { get; set; }

	IListField Field { get; set; }

	public void Setup(IListField _Field, int _Index)
	{
		Field = _Field;
		Index = _Index;
		
		Refresh();
	}

	protected abstract void Refresh();

	protected void Remove()
	{
		Field.Remove(Index);
	}

	protected void Modify()
	{
		Field.Modify(Index);
	}

	protected T GetValue<T>()
	{
		return (T)Field[Index];
	}

	protected void SetValue<T>(T _Value)
	{
		Field[Index] = _Value;
		
		Modify();
	}
}

public abstract class UIListEntry<T> : UIListEntry
{
	protected T Value
	{
		get => GetValue<T>();
		set => SetValue(value);
	}

	[SerializeField] TMP_Text       m_Label;
	[SerializeField] TMP_InputField m_Value;
	[SerializeField] Button         m_RemoveButton;

	protected override void Awake()
	{
		base.Awake();
		
		m_Value.onEndEdit.AddListener(Submit);
		m_RemoveButton.onClick.AddListener(Remove);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Value.onEndEdit.RemoveListener(Submit);
		m_RemoveButton.onClick.RemoveListener(Remove);
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		base.Reset();
		
		m_Label = Transform.Find("label").GetComponent<TMP_Text>();
		m_Value = Transform.Find("value").GetComponent<TMP_InputField>();
	}
	#endif

	protected override void Refresh()
	{
		m_Label.text = Index.ToString();
		m_Value.SetTextWithoutNotify(ParseString(Value));
	}

	void Submit(string _Text)
	{
		Value = ParseValue(_Text);
		
		Modify();
	}

	protected abstract string ParseString(T _Value);

	protected abstract T ParseValue(string _Text);
}