using UnityEngine;
using UnityEngine.UI;

public class UILevel : UIEntity
{
	public int Level
	{
		get => m_Level;
		set
		{
			if (m_Level == value)
				return;
			
			m_Level = value;
			
			ProcessLevel();
		}
	}

	[SerializeField] int      m_Level;
	[SerializeField] Image    m_Graphic;
	[SerializeField] Sprite[] m_Levels;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessLevel();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessLevel();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessLevel();
	}
	#endif

	void ProcessLevel()
	{
		int index = Mathf.Clamp(m_Level - 1, 0, m_Levels.Length - 1);
		
		m_Graphic.sprite = m_Levels[index];
	}
}