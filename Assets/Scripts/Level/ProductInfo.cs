using UnityEngine;

[CreateAssetMenu(fileName = "Purchase Info", menuName = "Registry/Purchase Info")]
public class ProductInfo : ScriptableObject
{
	public string      ID         => m_ID;
	public bool        Active     => m_Active;
	public LevelInfo[] LevelInfos => m_LevelInfos;

	[SerializeField] string      m_ID;
	[SerializeField] bool        m_Active;
	[SerializeField] LevelInfo[] m_LevelInfos;
}