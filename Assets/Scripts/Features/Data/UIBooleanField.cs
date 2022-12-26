using TMPro;
using UnityEngine;

public class UIBooleanField : UIField<bool>
{
	[SerializeField] TMP_Text        m_Label;
	[SerializeField] UIOverlayToggle m_Value;
	[SerializeField] UIGroup         m_Changed;

	protected override void Awake()
	{
		base.Awake();
		
		m_Value.ValueChanged.AddListener(Submit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Value.ValueChanged.RemoveListener(Submit);
	}

	protected override void Refresh()
	{
		m_Label.text  = Name;
		m_Value.Value = Value;
		
		if (m_Changed == null)
			return;
		
		if (Changed)
			m_Changed.Show();
		else
			m_Changed.Hide();
	}

	void Submit(bool _Value)
	{
		Value = _Value;
		
		Refresh();
	}
}