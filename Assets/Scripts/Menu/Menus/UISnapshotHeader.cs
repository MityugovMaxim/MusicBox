using TMPro;
using UnityEngine;

public class UISnapshotHeader : UIEntity
{
	[SerializeField] TMP_Text m_Title;

	public void Setup(string _Title)
	{
		m_Title.text = _Title;
	}
}