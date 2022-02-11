using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UILevelRating : UIEntity
{
	[SerializeField] Toggle m_LikeButton;
	[SerializeField] Toggle m_DislikeButton;

	HapticProcessor    m_HapticProcessor;
	StatisticProcessor m_StatisticProcessor;

	int    m_Rating;
	string m_LevelID;

	[Inject]
	public void Construct(
		HapticProcessor    _HapticProcessor,
		StatisticProcessor _StatisticProcessor
	)
	{
		m_HapticProcessor    = _HapticProcessor;
		m_StatisticProcessor = _StatisticProcessor;
	}

	public void Setup(string _LevelID)
	{
		m_LevelID = _LevelID;
		
		m_Rating = GetRating();
		
		m_LikeButton.SetIsOnWithoutNotify(m_Rating == 1);
		m_DislikeButton.SetIsOnWithoutNotify(m_Rating == -1);
	}

	public void Like(bool _Value)
	{
		m_Rating = _Value ? 1 : 0;
		
		m_HapticProcessor.Process(Haptic.Type.Success);
		
		m_LikeButton.SetIsOnWithoutNotify(m_Rating == 1);
		m_DislikeButton.SetIsOnWithoutNotify(m_Rating == -1);
	}

	public void Dislike(bool _Value)
	{
		m_Rating = _Value ? -1 : 0;
		
		m_HapticProcessor.Process(Haptic.Type.Warning);
		
		m_LikeButton.gameObject.SetActive(m_Rating == 1);
		m_DislikeButton.gameObject.SetActive(m_Rating == -1);
	}

	public void Execute()
	{
		int rating = GetRating();
		
		if (m_Rating == rating)
			return;
		
		SetRating(m_Rating);
		
		int delta = m_Rating - rating;
		
		m_StatisticProcessor.LogResultMenuControlPageRating(m_LevelID, delta);
	}

	int GetRating()
	{
		return PlayerPrefs.GetInt($"rating_{m_LevelID}", 0);
	}

	void SetRating(int _Rating)
	{
		PlayerPrefs.SetInt($"rating_{m_LevelID}", _Rating);
	}
}