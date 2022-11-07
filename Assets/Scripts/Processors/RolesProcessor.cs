using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class RoleSnapshot : Snapshot
{
	public string Name      { get; }
	public bool   Roles     { get; }
	public bool   Ambient   { get; }
	public bool   Languages { get; }
	public bool   Banners   { get; }
	public bool   Ads       { get; }
	public bool   Songs     { get; }
	public bool   Progress  { get; }
	public bool   Revives   { get; }
	public bool   Offers    { get; }
	public bool   News      { get; }
	public bool   Products  { get; }
	public bool   Daily     { get; }
	public bool   Vouchers  { get; }

	public RoleSnapshot() : base("new_role_user", 0)
	{
		Name      = string.Empty;
		Roles     = false;
		Ambient   = false;
		Languages = false;
		Banners   = false;
		Songs     = false;
		Ads       = false;
		Progress  = false;
		Revives   = false;
		Offers    = false;
		News      = false;
		Products  = false;
		Daily     = false;
		Vouchers  = false;
	}

	public RoleSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Name      = _Data.GetString("name");
		Roles     = _Data.GetBool("permissions/roles");
		Songs     = _Data.GetBool("permissions/songs");
		Progress  = _Data.GetBool("permissions/progress");
		Revives   = _Data.GetBool("permissions/revives");
		Offers    = _Data.GetBool("permissions/offers");
		Ads       = _Data.GetBool("permissions/ads");
		News      = _Data.GetBool("permissions/news");
		Ambient   = _Data.GetBool("permissions/ambient");
		Languages = _Data.GetBool("permissions/languages");
		Banners   = _Data.GetBool("permissions/banners");
		Products  = _Data.GetBool("permissions/products");
		Daily     = _Data.GetBool("permissions/daily");
		Vouchers  = _Data.GetBool("permissions/vouchers");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		base.Serialize(_Data);
		
		_Data["name"] = Name;
		_Data["permissions"] = new Dictionary<string, bool>()
		{
			{ "roles", Roles },
			{ "songs", Songs },
			{ "progress", Progress },
			{ "revives", Revives },
			{ "offers", Offers },
			{ "news", News },
			{ "ads", Ads },
			{ "ambient", Ambient },
			{ "languages", Languages },
			{ "banners", Banners },
			{ "products", Products },
			{ "daily", Daily },
			{ "vouchers", Vouchers },
		};
	}

	public override string ToString() => Name;
}

[Preserve]
public class RolesDataUpdateSignal { }

[Preserve]
public class RolesProcessor : DataProcessor<RoleSnapshot, RolesDataUpdateSignal>
{
	protected override string Path => "roles";

	protected override bool SupportsDevelopment => false;

	[Inject] SocialProcessor m_SocialProcessor;

	protected override Task OnFetch()
	{
		return FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true);
	}

	protected override Task OnUpdate()
	{
		return FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(true);
	}

	public string GetName() => GetName(m_SocialProcessor.UserID);

	public string GetName(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Name;
	}

	public bool HasRolesPermission() => HasRolesPermission(m_SocialProcessor.UserID);

	public bool HasRolesPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Roles ?? false;
	}

	public bool HasAmbientPermission() => HasAmbientPermission(m_SocialProcessor.UserID);

	public bool HasAmbientPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Ambient ?? false;
	}

	public bool HasLanguagesPermission() => HasLanguagesPermission(m_SocialProcessor.UserID);

	public bool HasLanguagesPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Languages ?? false;
	}

	public bool HasAdsPermission() => HasAdsPermission(m_SocialProcessor.UserID);

	public bool HasAdsPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Ads ?? false;
	}

	public bool HasBannersPermission() => HasBannersPermission(m_SocialProcessor.UserID);

	public bool HasBannersPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Banners ?? false;
	}

	public bool HasSongsPermission() => HasSongsPermission(m_SocialProcessor.UserID);

	public bool HasSongsPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Songs ?? false;
	}

	public bool HasProgressPermission() => HasProgressPermission(m_SocialProcessor.UserID);


	public bool HasProgressPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Progress ?? false;
	}

	public bool HasRevivesPermission() => HasRevivesPermission(m_SocialProcessor.UserID);

	public bool HasRevivesPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Revives ?? false;
	}

	public bool HasOffersPermission() => HasOffersPermission(m_SocialProcessor.UserID);

	public bool HasOffersPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Offers ?? false;
	}

	public bool HasNewsPermission() => HasNewsPermission(m_SocialProcessor.UserID);

	public bool HasNewsPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.News ?? false;
	}

	public bool HasProductsPermission() => HasProductsPermission(m_SocialProcessor.UserID);

	public bool HasProductsPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Products ?? false;
	}

	public bool HasDailyPermission() => HasDailyPermission(m_SocialProcessor.UserID);

	public bool HasDailyPermission(string _RoleID)
	{
		RoleSnapshot snapshot = GetSnapshot(_RoleID);
		
		return snapshot?.Daily ?? false;
	}
}
