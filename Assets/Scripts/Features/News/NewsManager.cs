using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class NewsManager : IDataManager
{
	public NewsCollection Collection => m_NewsCollection;
	public NewsDescriptor Descriptor => m_NewsDescriptor;

	[Inject] NewsCollection m_NewsCollection;
	[Inject] NewsDescriptor m_NewsDescriptor;

	public Task<bool> Activate()
	{
		return GroupTask.ProcessAsync(
			this,
			Collection.Load,
			Descriptor.Load
		);
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

	public long GetTimestamp(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		return snapshot?.Timestamp ?? 0;
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = Collection.GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}
}
