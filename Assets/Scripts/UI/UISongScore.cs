using UnityEngine;
using Zenject;

public class UISongScore : UISongEntity
{
	[SerializeField] GameObject  m_Content;
	[SerializeField] UIUnitLabel m_Score;

	[Inject] ScoresManager m_ScoresManager;

	protected override void Subscribe()
	{
		m_ScoresManager.Profile.Subscribe(DataEventType.Add, SongID, ProcessData);
		m_ScoresManager.Profile.Subscribe(DataEventType.Remove, SongID, ProcessData);
		m_ScoresManager.Profile.Subscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void Unsubscribe()
	{
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Add, SongID, ProcessData);
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Remove, SongID, ProcessData);
		m_ScoresManager.Profile.Unsubscribe(DataEventType.Change, SongID, ProcessData);
	}

	protected override void ProcessData()
	{
		m_Content.SetActive(m_ScoresManager.Profile.Contains(SongID));
		m_Score.Value = m_ScoresManager.GetScore(SongID);
	}
}