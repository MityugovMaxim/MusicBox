using UnityEngine;

public class UIOfferRewardSong : UIOfferReward
{
	protected override RewardType Type => RewardType.Song;

	[SerializeField] UISongItem m_Song;

	protected override void ProcessSong(string _SongID)
	{
		SetActive(!string.IsNullOrEmpty(_SongID));
		
		m_Song.Setup(_SongID);
	}
}
