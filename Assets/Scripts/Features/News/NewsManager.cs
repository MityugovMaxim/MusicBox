using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class NewsManager
{
	public NewsCollection Collection => m_NewsCollection;

	[Inject] NewsCollection m_NewsCollection;
	[Inject] NewsDescriptor m_NewsDescriptor;

	public List<string> GetNewsIDs()
	{
		return m_NewsCollection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	public bool IsActive(string _NewsID)
	{
		NewsSnapshot snapshot = m_NewsCollection.GetSnapshot(_NewsID);
		
		return snapshot?.Active ?? false;
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = m_NewsCollection.GetSnapshot(_NewsID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _NewsID) => m_NewsDescriptor.GetTitle(_NewsID);

	public string GetDescription(string _NewsID) => m_NewsDescriptor.GetDescription(_NewsID);

	public string GetDate(string _NewsID)
	{
		NewsSnapshot snapshot = m_NewsCollection.GetSnapshot(_NewsID);
		
		if (snapshot == null || snapshot.Timestamp == 0)
			return string.Empty;
		
		DateTime date = TimeUtility.GetLocalTime(snapshot.Timestamp);
		
		return date.ToShortDateString();
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = m_NewsCollection.GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}
}
