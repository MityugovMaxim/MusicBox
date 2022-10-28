using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.Logging;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class BannerSnapshot : Snapshot
{
	public bool   Active    { get; }
	public string Image     { get; }
	public string Language  { get; }
	public bool   Permanent { get; }
	public string URL       { get; }

	public BannerSnapshot() : base("new_banner", 0)
	{
		Active    = false;
		Image     = "Thumbnails/Banners/new_banner.jpg";
		Language  = "en";
		Permanent = false;
		URL       = "audiobox://";
	}

	public BannerSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/Banners/{ID}.jpg");
		Language  = _Data.GetString("language");
		Permanent = _Data.GetBool("permanent");
		URL       = _Data.GetString("url");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["active"]    = Active;
		_Data["language"]  = Language;
		_Data["permanent"] = Permanent;
		_Data["url"]       = URL;
	}
}

[Preserve]
public class BannersDataUpdateSignal { }

[Preserve]
public class BannersProcessor : DataProcessor<BannerSnapshot, BannersDataUpdateSignal>, IInitializable, IDisposable
{
	protected override string Path => $"banners/{m_LanguageProcessor.Language}";

	protected override bool SupportsDevelopment => true;

	[Inject] LanguageProcessor m_LanguageProcessor;

	void IInitializable.Initialize()
	{
		SignalBus.Subscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	void IDisposable.Dispose()
	{
		SignalBus.Unsubscribe<LanguageSelectSignal>(OnLanguageSelect);
	}

	public List<string> GetBannerIDs()
	{
		return Snapshots
			.Where(_Snapshot => _Snapshot != null)
			.Where(_Snapshot => _Snapshot.Active)
			.Select(_Snapshot => _Snapshot.ID)
			.ToList();
	}

	public string GetImage(string _BannerID)
	{
		BannerSnapshot snapshot = GetSnapshot(_BannerID);
		
		return snapshot?.Image ?? string.Empty;
	}

	public string GetURL(string _BannerID)
	{
		BannerSnapshot snapshot = GetSnapshot(_BannerID);
		
		if (snapshot == null)
		{
			Log.Error(this, "Get URL failed. Snapshot with ID '{0}' is null.", _BannerID);
			return string.Empty;
		}
		
		return snapshot.URL;
	}

	public bool IsPermanent(string _BannerID)
	{
		BannerSnapshot snapshot = GetSnapshot(_BannerID);
		
		if (snapshot == null)
		{
			Log.Error(this, "Check permanent failed. Snapshot with ID '{0}' is null.", _BannerID);
			return false;
		}
		
		return snapshot.Permanent;
	}

	async void OnLanguageSelect()
	{
		Unload();
		
		await Load();
		
		SignalBus.Fire<BannersDataUpdateSignal>();
	}
}
