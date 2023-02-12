using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIResultChests : UIGroup
{
	[SerializeField] UIChestIndicator m_BronzeChest;
	[SerializeField] UIChestIndicator m_SilverChest;
	[SerializeField] UIChestIndicator m_GoldChest;
	[SerializeField] UIChestIndicator m_PlatinumChest;

	[SerializeField] UIResultDisc m_BronzeDisc;
	[SerializeField] UIResultDisc m_SilverDisc;
	[SerializeField] UIResultDisc m_GoldDisc;
	[SerializeField] UIResultDisc m_PlatinumDisc;

	[Inject] ScoreController m_ScoreController;
	[Inject] ScoresManager   m_ScoresManager;

	string   m_SongID;
	RankType m_SourceScoreRank;
	RankType m_TargetScoreRank;

	public void Setup(string _SongID)
	{
		m_SongID          = _SongID;
		m_SourceScoreRank = m_ScoresManager.GetRank(m_SongID);
		m_TargetScoreRank = m_ScoreController.GetRank();
	}

	public async Task PlayAsync()
	{
		if (m_SourceScoreRank >= m_TargetScoreRank)
			return;
		
		for (RankType rank = m_SourceScoreRank + 1; rank <= m_TargetScoreRank; rank++)
		{
			UIResultDisc disc = GetDisc(rank);
			if (disc != null)
				await disc.CollectAsync();
			
			UIChestIndicator chest = GetChest(rank);
			if (chest != null)
				chest.Progress();
			
			await Task.Delay(100);
		}
	}

	UIChestIndicator GetChest(RankType _ScoreRank)
	{
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return m_BronzeChest;
			case RankType.Silver:   return m_SilverChest;
			case RankType.Gold:     return m_GoldChest;
			case RankType.Platinum: return m_PlatinumChest;
			default:                return null;
		}
	}

	UIResultDisc GetDisc(RankType _ScoreRank)
	{
		switch (_ScoreRank)
		{
			case RankType.Bronze:   return m_BronzeDisc;
			case RankType.Silver:   return m_SilverDisc;
			case RankType.Gold:     return m_GoldDisc;
			case RankType.Platinum: return m_PlatinumDisc;
			default:                return null;
		}
	}
}
