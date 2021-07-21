using TMPro;
using UnityEngine;

public class UITrackInfo : MonoBehaviour
{
	[SerializeField] TMP_Text m_Label;

	public void Initialize(string _Title, string _Artist)
	{
		if (m_Label == null)
		{
			Debug.LogErrorFormat(gameObject, "[UITrackInfo] Initialize failed. Label is not assiged.");
			return;
		}
		
		m_Label.text = $"<b>{_Title}</b>\n<size=32><color=#a0a0a0>{_Artist}</color></size>";
	}
}
