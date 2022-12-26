using UnityEngine;

public class UIDiscs : UIGroup
{
	public RankType Rank
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

	[SerializeField] RankType  m_Rank;
	[SerializeField] GameObject m_BronzeRank;
	[SerializeField] GameObject m_SilverRank;
	[SerializeField] GameObject m_GoldRank;
	[SerializeField] GameObject m_PlatinumRank;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessRank();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		
		ProcessRank();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		ProcessRank();
	}
	#endif

	void ProcessRank()
	{
		m_BronzeRank.SetActive(Rank >= RankType.Bronze);
		m_SilverRank.SetActive(Rank >= RankType.Silver);
		m_GoldRank.SetActive(Rank >= RankType.Gold);
		m_PlatinumRank.SetActive(Rank >= RankType.Platinum);
	}
}