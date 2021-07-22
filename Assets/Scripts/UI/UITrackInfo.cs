using TMPro;
using UnityEngine;
using Zenject;

public class UITrackInfo : MonoBehaviour
{
	[SerializeField] TMP_Text m_Label;

	LevelProvider m_LevelProvider;

	[Inject]
	public void Construct(LevelProvider _LevelProvider)
	{
		m_LevelProvider = _LevelProvider;
		
		if (m_LevelProvider == null)
		{
			Debug.LogError("[UITrackInfo] Construct failed. Level provider is null.");
			return;
		}
		
		m_LevelProvider.LevelChanged += LevelChanged;
	}

	void LevelChanged()
	{
		if (m_Label != null && m_LevelProvider != null)
			m_Label.text = $"<b>{m_LevelProvider.Title}</b>\n<size=32><color=#a0a0a0>{m_LevelProvider.Artist}</color></size>";
	}
}
