using UnityEngine;
using Zenject;

public class UISongDiscs : UIEntity
{
	public ScoreRank Rank
	{
		get => m_Rank;
		set
		{
			if (m_Rank == value)
				return;
			
			m_Rank = value;
			
			ProcessRank();
		}
	}

	[SerializeField] ScoreRank  m_Rank;
	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	[Inject] ScoresProcessor m_ScoresProcessor;

	string m_SongID;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessRank();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessRank();
	}
	#endif

	public void Setup(string _SongID)
	{
		m_SongID = _SongID;
		
		m_Rank = m_ScoresProcessor.GetRank(m_SongID);
		
		ProcessRank();
	}

	void ProcessRank()
	{
		gameObject.SetActive(Rank >= ScoreRank.None);
		m_PlatinumRank.SetActive(Rank >= ScoreRank.Platinum);
		m_GoldRank.SetActive(Rank >= ScoreRank.Gold);
		m_SilverRank.SetActive(Rank >= ScoreRank.Silver);
		m_BronzeRank.SetActive(Rank >= ScoreRank.Bronze);
	}
}
