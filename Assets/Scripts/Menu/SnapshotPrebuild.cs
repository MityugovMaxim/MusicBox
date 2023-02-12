using System;
using System.Collections.Generic;
using System.Reflection;
using Firebase.Database;

public class SnapshotPrebuild
{
	delegate Snapshot CreateDelegate(DataSnapshot _Data);

	static readonly Dictionary<Type, CreateDelegate> m_Functions = new Dictionary<Type, CreateDelegate>()
	//// PREBUILD_START
	{
		{ typeof(AdsProviderSnapshot), _Data => new AdsProviderSnapshot(_Data) },
		{ typeof(AmbientSnapshot), _Data => new AmbientSnapshot(_Data) },
		{ typeof(ChestSlot), _Data => new ChestSlot(_Data) },
		{ typeof(ChestSnapshot), _Data => new ChestSnapshot(_Data) },
		{ typeof(ComboSnapshot), _Data => new ComboSnapshot(_Data) },
		{ typeof(DailySnapshot), _Data => new DailySnapshot(_Data) },
		{ typeof(Descriptor), _Data => new Descriptor(_Data) },
		{ typeof(DifficultySnapshot), _Data => new DifficultySnapshot(_Data) },
		{ typeof(FrameSnapshot), _Data => new FrameSnapshot(_Data) },
		{ typeof(ProfileFrame), _Data => new ProfileFrame(_Data) },
		{ typeof(LanguageSnapshot), _Data => new LanguageSnapshot(_Data) },
		{ typeof(NewsSnapshot), _Data => new NewsSnapshot(_Data) },
		{ typeof(OfferSnapshot), _Data => new OfferSnapshot(_Data) },
		{ typeof(ProfileOffer), _Data => new ProfileOffer(_Data) },
		{ typeof(ProductSnapshot), _Data => new ProductSnapshot(_Data) },
		{ typeof(ProfileProduct), _Data => new ProfileProduct(_Data) },
		{ typeof(PurchaseSnapshot), _Data => new PurchaseSnapshot(_Data) },
		{ typeof(ProfileDailyData), _Data => new ProfileDailyData(_Data) },
		{ typeof(ProfileScore), _Data => new ProfileScore(_Data) },
		{ typeof(ProfileSeason), _Data => new ProfileSeason(_Data) },
		{ typeof(SeasonItem), _Data => new SeasonItem(_Data) },
		{ typeof(SeasonLevel), _Data => new SeasonLevel(_Data) },
		{ typeof(SeasonSnapshot), _Data => new SeasonSnapshot(_Data) },
		{ typeof(ProfileSongData), _Data => new ProfileSongData(_Data) },
		{ typeof(SongSnapshot), _Data => new SongSnapshot(_Data) },
		{ typeof(StoreSnapshot), _Data => new StoreSnapshot(_Data) },
		{ typeof(TimerSnapshot), _Data => new TimerSnapshot(_Data) },
		{ typeof(ProfileVoucher), _Data => new ProfileVoucher(_Data) },
		{ typeof(VoucherSnapshot), _Data => new VoucherSnapshot(_Data) },
		{ typeof(BannerSnapshot), _Data => new BannerSnapshot(_Data) },
		{ typeof(ColorsSnapshot), _Data => new ColorsSnapshot(_Data) },
		{ typeof(ProgressSnapshot), _Data => new ProgressSnapshot(_Data) },
		{ typeof(ReviveSnapshot), _Data => new ReviveSnapshot(_Data) },
		{ typeof(RoleSnapshot), _Data => new RoleSnapshot(_Data) },
	};

	//// PREBUILD_END

	public static T Create<T>(DataSnapshot _Data) where T : Snapshot
	{
		Type type = typeof(T);
		if (m_Functions.TryGetValue(type, out CreateDelegate function) && function != null)
			return function.Invoke(_Data) as T;
		return Activator.CreateInstance(type, _Data) as T;
	}

	public static Type[] GetSnapshotTypes()
	{
		List<Type> types = new List<Type>();
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		foreach (Type type in assembly.GetTypes())
		{
			if (type.IsAbstract || type.IsGenericType)
				continue;
				
			if (type.IsSubclassOf(typeof(Snapshot)))
				types.Add(type);
		}
		return types.ToArray();
	}
}
