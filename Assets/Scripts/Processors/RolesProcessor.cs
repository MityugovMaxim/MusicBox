using System.Collections.Generic;
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
	public bool   Songs     { get; }
	public bool   Progress  { get; }
	public bool   Offers    { get; }
	public bool   News      { get; }
	public bool   Products  { get; }

	protected RoleSnapshot(DataSnapshot _Data) : base(_Data)
	{
		Name      = _Data.GetString("name");
		Roles     = _Data.GetBool("permissions/roles");
		Songs     = _Data.GetBool("permissions/songs");
		Progress  = _Data.GetBool("permissions/progress");
		Offers    = _Data.GetBool("permissions/offers");
		News      = _Data.GetBool("permissions/news");
		Ambient   = _Data.GetBool("permissions/ambient");
		Languages = _Data.GetBool("permissions/languages");
		Banners   = _Data.GetBool("permissions/banners");
		Products  = _Data.GetBool("permissions/products");
	}

	public override void Serialize(Dictionary<string, object> _Data)
	{
		_Data["name"] = Name;
		_Data["permissions"] = new Dictionary<string, bool>()
		{
			{ "roles", Roles },
			{ "songs", Songs },
			{ "progress", Progress },
			{ "offers", Offers },
			{ "news", News },
			{ "ambient", Ambient },
			{ "languages", Languages },
			{ "banners", Banners },
			{ "products", Products },
		};
	}
}

[Preserve]
public class RolesDataUpdateSignal { }

[Preserve]
public class RolesProcessor : DataProcessor<RoleSnapshot, RolesDataUpdateSignal>
{
	protected override string Path => $"roles";

	[Inject] SocialProcessor m_SocialProcessor;

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
}