using UnityEngine;
using Zenject;

public class UISongDiscount : UISongEntity
{
	[SerializeField] GameObject  m_Content;
	[SerializeField] UIUnitLabel m_Discount;

	[Inject] VouchersManager m_VouchersManager;

	protected override void Subscribe()
	{
		m_VouchersManager.Profile.Subscribe(DataEventType.Add, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Remove, ProcessData);
		m_VouchersManager.Profile.Subscribe(DataEventType.Change, ProcessData);
		SongsManager.Collection.Subscribe(DataEventType.Change, ProcessData);
	}

	protected override void Unsubscribe()
	{
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Add, ProcessData);
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Remove, ProcessData);
		m_VouchersManager.Profile.Unsubscribe(DataEventType.Change, ProcessData);
		SongsManager.Collection.Unsubscribe(DataEventType.Change, ProcessData);
	}

	protected override void ProcessData()
	{
		long source = SongsManager.GetPrice(SongID);
		long target = m_VouchersManager.GetSongDiscount(SongID, source);
		
		m_Content.SetActive(source > target);
		m_Discount.Value = source;
	}
}