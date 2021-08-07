using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelLike : UIEntity
{
	[SerializeField] Button m_LikeButton;
	[SerializeField] Button m_DislikeButton;

	StatisticProcessor m_StatisticProcessor;

	bool   m_State;
	bool   m_Liked;
	string m_LevelID;

	[Inject]
	public void Construct(StatisticProcessor _StatisticProcessor)
	{
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _LevelID)
	{
		Debug.LogError("---> SETUP LEVEL LIKE");
		
		m_LevelID = _LevelID;
		
		m_Liked = PlayerPrefs.GetInt($"like_{m_LevelID}", 0) > 0;
		m_State = m_Liked;
		
		if (m_LikeButton != null)
			m_LikeButton.gameObject.SetActive(!m_State);
		if (m_DislikeButton != null)
			m_DislikeButton.gameObject.SetActive(m_State);
	}

	public void Execute()
	{
		if (m_State == m_Liked)
			return;
		
		m_State = m_Liked;
		
		PlayerPrefs.SetInt($"like_{m_LevelID}", m_Liked ? 1 : 0);
		
		if (m_Liked)
			m_StatisticProcessor.LogLevelLike(m_LevelID);
		else
			m_StatisticProcessor.LogLevelDislike(m_LevelID);
	}

	public void Like()
	{
		if (m_Liked)
			return;
		
		m_Liked = true;
		
		m_LikeButton.gameObject.SetActive(false);
		m_DislikeButton.gameObject.SetActive(true);
	}

	public void Dislike()
	{
		if (!m_Liked)
			return;
		
		m_Liked = false;
		
		m_LikeButton.gameObject.SetActive(true);
		m_DislikeButton.gameObject.SetActive(false);
	}
}