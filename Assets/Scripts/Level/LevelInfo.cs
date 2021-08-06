using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "Registry/Level Info")]
public class LevelInfo : RegistryEntry
{
	public string    ID            => m_ID;
	public string    Title         => m_Title;
	public string    Artist        => m_Artist;
	public string    LeaderboardID => m_LeaderboardID;
	public LevelMode Mode          => m_Mode;

	[SerializeField] string    m_ID;
	[SerializeField] string    m_Title;
	[SerializeField] string    m_Artist;
	[SerializeField] string    m_LeaderboardID;
	[SerializeField] LevelMode m_Mode;
}