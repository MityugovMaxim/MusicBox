using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Registry", menuName = "Level Registry")]
public class LevelRegistry : ScriptableObject, IEnumerable<LevelInfo>
{
	public LevelInfo this[int _Index] => m_LevelInfos[_Index];

	public int Length => m_LevelInfos.Count;

	[SerializeField] List<LevelInfo> m_LevelInfos;

	public IEnumerator<LevelInfo> GetEnumerator()
	{
		return m_LevelInfos.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}