using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

public class NewsSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public long   Timestamp { get; }
	public string URL       { get; }

	public NewsSnapshot() : base("new_news", 0)
	{
		Active    = false;
		Image     = "Thumbnails/News/new_news.jpg";
		Timestamp = TimeUtility.GetTimestamp();
		URL       = "audiobox://";
	}

	public NewsSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/News/{ID}.jpg");
		Timestamp = _Data.GetLong("timestamp");
		URL       = _Data.GetString("url");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]    = Active;
		_Data["image"]     = Image;
		_Data["timestamp"] = Timestamp;
		_Data["url"]       = URL;
	}
}

[Preserve]
public class NewsDataUpdateSignal { }

[Preserve]
public class NewsDescriptor : DescriptorProcessor<NewsDataUpdateSignal>
{
	protected override string Path => "news_descriptors";
}

[Preserve]
public class NewsProcessor : DataProcessor<NewsSnapshot, NewsDataUpdateSignal>
{
	protected override string Path => "news";

	[Inject] NewsDescriptor m_NewsDescriptor;

	protected override Task OnFetch() => m_NewsDescriptor.Load();

	public List<string> GetNewsIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.OrderByDescending(_Snapshot => _Snapshot.Timestamp)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetTitle(string _NewsID) => m_NewsDescriptor.GetTitle(_NewsID);

	public string GetDescription(string _NewsID) => m_NewsDescriptor.GetDescription(_NewsID);

	public string GetDate(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		if (snapshot == null || snapshot.Timestamp == 0)
			return string.Empty;
		
		DateTime date = TimeUtility.GetLocalTime(snapshot.Timestamp);
		
		return date.ToShortDateString();
	}

	public string GetURL(string _NewsID)
	{
		NewsSnapshot snapshot = GetSnapshot(_NewsID);
		
		return snapshot?.URL ?? string.Empty;
	}
}
