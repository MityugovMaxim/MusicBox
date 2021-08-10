using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "Registry/Level Info")]
public class LevelInfo : RegistryEntry
{
	public string    Title         => m_Title;
	public string    Artist        => m_Artist;
	public string    ID            => m_ID;
	public string    LeaderboardID => m_LeaderboardID;
	public string    AchievementID => m_AchievementID;
	public LevelMode Mode          => m_Mode;
	public bool      Locked        => m_Locked;
	public int       EXP           => m_EXP;

	[SerializeField, HideInInspector] string    m_Artist;
	[SerializeField, HideInInspector] string    m_Title;
	[SerializeField, HideInInspector] string    m_ID;
	[SerializeField, HideInInspector] string    m_LeaderboardID;
	[SerializeField, HideInInspector] string    m_AchievementID;
	[SerializeField, HideInInspector] LevelMode m_Mode;
	[SerializeField, HideInInspector] bool      m_Locked;
	[SerializeField, HideInInspector] int       m_EXP;
}