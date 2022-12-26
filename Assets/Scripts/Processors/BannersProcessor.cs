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

	public BannerSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Active    = _Data.GetBool("active");
		Image     = _Data.GetString("image", $"Thumbnails/Banners/{ID}.jpg");
		Language  = _Data.GetString("language");
		Permanent = _Data.GetBool("permanent");
		URL       = _Data.GetString("url");
	}
}

[Preserve]
public class BannersProcessor : DataCollection<BannerSnapshot>
{
	protected override string Path => $"banners/{m_LanguagesManager.Language}";

	[Inject] LanguagesManager m_LanguagesManager;

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
}
