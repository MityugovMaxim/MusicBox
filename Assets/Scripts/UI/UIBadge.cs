using TMPro;
using UnityEngine;

public class UIBadge : UIGroup
{
	public int Value
	{
		get => m_Value;
		set
		{
			if (m_Value == value)
				return;
			
			m_Value = value;
			
			ProcessValue();
		}
	}

	[SerializeField] TMP_Text m_Label;
	[SerializeField] int      m_Value;

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessValue();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessValue();
	}
	#endif

	void ProcessValue()
	{
		if (Value <= 0)
		{
			Hide();
			return;
		}
		
		int value = Mathf.Max(1, Value);
		
		m_Label.text = value <= 9 ? value.ToString() : "9+";
		
		Show();
	}
}
