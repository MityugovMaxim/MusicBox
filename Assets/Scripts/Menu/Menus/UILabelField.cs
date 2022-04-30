using TMPro;
using UnityEngine;

public class UILabelField : UIEntity
{
	public string Value
	{
		get => m_Label.text;
		set => m_Label.text = value;
	}

	[SerializeField] TMP_Text m_Label;
}