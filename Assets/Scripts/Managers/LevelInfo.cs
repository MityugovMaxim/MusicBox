using UnityEngine;

[CreateAssetMenu(fileName = "Level Info", menuName = "Level Info")]
public class LevelInfo : ScriptableObject
{
	public string ID     => m_ID;
	public string Title  => m_Title;
	public string Artist => m_Artist;

	[SerializeField] string m_ID;
	[SerializeField] string m_Title;
	[SerializeField] string m_Artist;
}