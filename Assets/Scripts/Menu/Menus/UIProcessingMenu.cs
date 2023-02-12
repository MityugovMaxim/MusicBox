using TMPro;
using UnityEngine;

[Menu(MenuType.ProcessingMenu)]
public class UIProcessingMenu : UIMenu
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

	[SerializeField] TMP_Text m_Label;
	[SerializeField] UIGroup  m_LabelGroup;

	string m_Text;

	public void SetProgress(string _Text, float _Progress)
	{
		m_Text = $"{_Text}\n{Mathf.RoundToInt(_Progress * 100)}%";
		
		m_Label.text = m_Text;
		
		m_LabelGroup.Show();
	}

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		m_Text = null;
		
		m_Label.text = string.Empty;
		
		m_LabelGroup.Hide(true);
	}

	async void ProcessText()
	{
		await m_LabelGroup.HideAsync();
		
		m_Label.text = m_Text;
		
		await m_LabelGroup.ShowAsync();
	}
}
