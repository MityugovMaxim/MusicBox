using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class NewsManager : IDataManager
{
	public bool           Activated  { get; private set; }
	public NewsCollection Collection => m_NewsCollection;
	public NewsDescriptor Descriptor => m_NewsDescriptor;

	[Inject] NewsCollection m_NewsCollection;
	[Inject] NewsDescriptor m_NewsDescriptor;

	public async Task<bool> Activate()
	{
		if (Activated)
			return true;
		
		int frame = Time.frameCount;
		
		await Task.WhenAll(
			Collection.Load(),
			Descriptor.Load()
		);
		
		Activated = true;
		
		return frame == Time.frameCount;
	}

	public List<string> GetNewsIDs()
	{
		return m_NewsCollection.GetIDs()
			.Where(IsActive)
			.ToList();
	}

	public bool IsActive(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		return snapshot?.Active ?? false;
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _NewsID) => Descriptor.GetTitle(_NewsID);

	public string GetDescription(string _NewsID) => Descriptor.GetDescription(_NewsID);

	public string GetDate(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		if (snapshot == null || snapshot.Timestamp == 0)
			return string.Empty;
		
		DateTime date = TimeUtility.GetLocalTime(snapshot.Timestamp);
		
		return date.ToShortDateString();
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}
}
