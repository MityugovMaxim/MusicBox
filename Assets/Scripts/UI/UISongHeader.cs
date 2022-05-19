using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

public class UISongHeader : UISongFrame
{
	[Preserve]
	public class Pool : UIEntityPool<UISongHeader> { }

	[SerializeField] TMP_Text m_Title;

	public void Setup(string _Title)
	{
		m_Title.text = _Title;
	}
}