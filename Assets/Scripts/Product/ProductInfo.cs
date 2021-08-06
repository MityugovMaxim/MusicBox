using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Purchase Info", menuName = "Registry/Purchase Info")]
public class ProductInfo : RegistryEntry
{
	public string      ID         => m_ID;
	public LevelInfo[] LevelInfos => m_LevelInfos;

	[SerializeField] string      m_ID;
	[SerializeField] LevelInfo[] m_LevelInfos;

	public bool ContainsLevel(string _LevelID)
	{
		return m_LevelInfos.Any(_LevelInfo => _LevelInfo.ID == _LevelID);
	}
}