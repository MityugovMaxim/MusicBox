using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "Level Info")]
public class LevelInfo : ScriptableObject
{
	public enum LevelMode
	{
		Free     = 0,
		Ads      = 1,
		Purchase = 2,
	}

	public string    ID     => m_ID;
	public string    Title  => m_Title;
	public string    Artist => m_Artist;
	public LevelMode Mode   => m_Mode;

	[SerializeField] string    m_ID;
	[SerializeField] string    m_Title;
	[SerializeField] string    m_Artist;
	[SerializeField] LevelMode m_Mode;
}