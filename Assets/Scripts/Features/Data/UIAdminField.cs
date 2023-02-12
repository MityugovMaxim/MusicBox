using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIAdminField : UIOverlayButton
{
	public string Value
	{
		get => m_Field.text;
		set => m_Field.SetTextWithoutNotify(value);
	}

	[SerializeField] TMP_InputField m_Field;

	protected override void Awake()
	{
		base.Awake();
		
		m_Field.enabled = false;
		
		m_Field.Subscribe(Deactivate);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Field.Unsubscribe(Deactivate);
	}

	public void AddListener(UnityAction<string> _Action) => m_Field.Subscribe(_Action);

	public void RemoveListener(UnityAction<string> _Action) => m_Field.Unsubscribe(_Action);

	protected override void OnClick()
	{
		base.OnClick();
		
		Activate();
	}

	void Activate()
	{
		m_Field.enabled = true;
		m_Field.ActivateInputField();
		m_Field.MoveToEndOfLine(false, true);
	}

	void Deactivate(string _Value = null)
	{
		m_Field.DeactivateInputField(true);
		m_Field.enabled = false;
	}
}
