using TMPro;
using UnityEngine;
using Zenject;

public class UILevelProgress : UIEntity
{
	const string PROGRESS_ICON = "progress_icon";

	[SerializeField] RectTransform m_Progress;
	[SerializeField] TMP_Text      m_Label;
	[SerializeField] float         m_Min = 0;
	[SerializeField] float         m_Max = 1;

	ProgressProcessor m_ProgressProcessor;

	[Inject]
	public void Construct(ProgressProcessor _ProgressProcessor)
	{
		m_ProgressProcessor = _ProgressProcessor;
	}

	public void Setup(string _LevelID)
	{
		bool levelLocked = m_ProgressProcessor.IsLevelLocked(_LevelID);
		
		gameObject.SetActive(levelLocked);
		
		if (!levelLocked)
			return;
		
		double expProgress = m_ProgressProcessor.ExpProgress;
		double expRequired = m_ProgressProcessor.GetExpRequired(_LevelID);
		
		float progress = Mathf.Clamp01((float)(expProgress / expRequired));
		
		Vector2 anchor = m_Progress.anchorMax;
		anchor.x = Mathf.Lerp(m_Min, m_Max, progress);
		m_Progress.anchorMax = anchor;
		
		m_Label.text = $"{expProgress}/{expRequired}<sprite name=\"{PROGRESS_ICON}\">";
	}
}