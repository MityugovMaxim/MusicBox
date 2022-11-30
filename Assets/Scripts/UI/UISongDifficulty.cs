using UnityEngine;

public class UISongDifficulty : UISongEntity
{
	[SerializeField] GameObject[] m_Difficulties;

	protected override void Subscribe()
	{
		SongsManager.Collection.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		SongsManager.Collection.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		DifficultyType difficulty = SongsManager.GetDifficulty(SongID);
		
		int index = (int)difficulty;
		
		for (int i = 0; i < m_Difficulties.Length; i++)
			m_Difficulties[i].SetActive(i == index);
	}
}
