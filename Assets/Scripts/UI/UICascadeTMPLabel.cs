using TMPro;
using UnityEngine;

public class UICascadeTMPLabel : UICascadeLabel
{
	public string Text
	{
		get => m_Text;
		set
		{
			if (m_Text == value)
				return;
			
			m_Text = value;
			
			ProcessText();
		}
	}

	protected override TMP_Text this[int _Index] => m_Labels[_Index];

	protected override int Count => m_Labels.Length;

	[SerializeField]           TMP_Text[] m_Labels;
	[SerializeField, TextArea] string     m_Text;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessText();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessText();
	}
	#endif

	void ProcessText()
	{
		foreach (TMP_Text label in m_Labels)
			label.text = Text;
	}
}