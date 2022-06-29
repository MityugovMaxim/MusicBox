using System.Collections.Generic;
using System.Globalization;
using AppsFlyerSDK;
using Facebook.Unity;
using Firebase.Analytics;
using UnityEngine.Analytics;
using UnityEngine.Scripting;
using Zenject;

public abstract class StatisticData
{
	public abstract Parameter GetParameter();
	public abstract void Fill(Dictionary<string, object> _Data);
	public abstract void Fill(Dictionary<string, string> _Data);

	public static StatisticData Create(string _Key, int _Value)
	{
		return new StatisticInteger(_Key, _Value);
	}
	
	public static StatisticData Create(string _Key, string _Value)
	{
		return new StatisticString(_Key, _Value);
	}
	
	public static StatisticData Create(string _Key, long _Value)
	{
		return new StatisticLong(_Key, _Value);
	}
	
	public static StatisticData Create(string _Key, double _Value)
	{
		return new StatisticDouble(_Key, _Value);
	}
	
	public static StatisticData Create(string _Key, bool _Value)
	{
		return new StatisticBool(_Key, _Value);
	}
}

public abstract class StatisticData<T> : StatisticData
{
	protected string Key   { get; }
	protected T      Value { get; }

	protected StatisticData(string _Key, T _Value)
	{
		Key   = _Key;
		Value = _Value;
	}

	public override void Fill(Dictionary<string, object> _Data)
	{
		_Data[Key] = Value;
	}

	public override void Fill(Dictionary<string, string> _Data)
	{
		_Data[Key] = Value.ToString();
	}
}

public class StatisticString : StatisticData<string>
{
	public StatisticString(string _Key, string _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value ?? "null");
	}
}

public class StatisticInteger : StatisticData<int>
{
	public StatisticInteger(string _Key, int _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}
}

public class StatisticLong : StatisticData<long>
{
	public StatisticLong(string _Key, long _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}
}

public class StatisticDouble : StatisticData<double>
{
	public StatisticDouble(string _Key, double _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}
}

public class StatisticBool : StatisticData<bool>
{
	public StatisticBool(string _Key, bool _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value ? 1 : 0);
	}
}

public interface IStatisticProvider
{
	void Purchase(string _ProductID, string _Currency, decimal _Price);

	void Log(string _Name, params StatisticData[] _Parameters);
}

[Preserve]
public class StatisticUnity : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			"unity_purchase",
			StatisticData.Create("product_id", _ProductID),
			StatisticData.Create("currency", _Currency),
			StatisticData.Create("price", (double)_Price)
		);
	}

	public void Log(string _Name, params StatisticData[] _Parameters)
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		if (_Parameters != null && _Parameters.Length > 0)
		{
			foreach (StatisticData parameter in _Parameters)
				parameter.Fill(data);
		}
		
		Analytics.CustomEvent(_Name, data);
	}
}

[Preserve]
public class StatisticFacebook : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		FB.LogPurchase(
			_Price,
			_Currency,
			new Dictionary<string, object>()
			{
				{ "product_id", _ProductID },
			}
		);
	}

	public void Log(string _Name, params StatisticData[] _Parameters)
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		
		if (_Parameters != null && _Parameters.Length > 0)
		{
			foreach (StatisticData parameter in _Parameters)
				parameter.Fill(data);
		}
		
		FB.LogAppEvent(_Name, null, data);
	}
}

[Preserve]
public class StatisticFirebase : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		FirebaseAnalytics.LogEvent(
			FirebaseAnalytics.EventPurchase,
			new Parameter("product_id", _ProductID),
			new Parameter("price", (double)_Price),
			new Parameter("currency", _Currency)
		);
	}

	public void Log(string _Name, params StatisticData[] _Parameters)
	{
		List<Parameter> data = new List<Parameter>();
		
		if (_Parameters != null && _Parameters.Length > 0)
		{
			foreach (StatisticData parameter in _Parameters)
				data.Add(parameter.GetParameter());
		}
		
		FirebaseAnalytics.LogEvent(_Name, data.ToArray());
	}
}

[Preserve]
public class StatisticAppsFlyer : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add(AFInAppEvents.CURRENCY, _Currency);
		data.Add(AFInAppEvents.REVENUE, _Price.ToString(CultureInfo.InvariantCulture));
		data.Add("af_quantity", "1");
		AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, data);
	}

	public void Log(string _Name, params StatisticData[] _Parameters)
	{
		Dictionary<string, string> data = new Dictionary<string, string>();
		
		if (_Parameters != null && _Parameters.Length > 0)
		{
			foreach (StatisticData parameter in _Parameters)
				parameter.Fill(data);
		}
		
		AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, data);
	}
}

public class StatisticProcessor
{
	readonly IStatisticProvider[] m_Providers;

	[Inject]
	public StatisticProcessor(IStatisticProvider[] _Providers)
	{
		m_Providers = _Providers;
	}

	#region Main Menu

	public void LogMainMenuPageSelect(MainMenuPageType _PageType)
	{
		LogEvent(
			"main_menu_control_bar_click",
			StatisticData.Create("page_type", _PageType.ToString())
		);
	}

	public void LogMainMenuProfileClick()
	{
		LogEvent("main_menu_profile_click");
	}

	public void LogMainMenuPromoClick(string _ProductID)
	{
		LogEvent(
			"main_menu_promo_click",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogSongItemClick(string _SongID)
	{
		LogEvent(
			"main_menu_songs_page_item_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogMainMenuStorePageItemClick(string _ProductID)
	{
		LogEvent(
			"main_menu_store_page_item_click",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogMainMenuNewsPageItemClick(string _NewsID)
	{
		LogEvent(
			"main_menu_news_page_item_click",
			StatisticData.Create("news_id", _NewsID)
		);
	}

	public void LogMainMenuOffersPageItemClick(string _OfferID)
	{
		LogEvent(
			"main_menu_offers_page_item_click",
			StatisticData.Create("offer_id", _OfferID)
		);
	}

	public void LogMainMenuProfilePageCoinsClick()
	{
		LogEvent("main_menu_profile_page_coins_click");
	}

	public void LogMainMenuProfilePageUsernameClick()
	{
		LogEvent("main_menu_profile_page_username_click");
	}

	public void LogMainMenuProfilePageLanguageClick(string _Language)
	{
		LogEvent(
			"main_menu_profile_page_language_click",
			StatisticData.Create("language", _Language)
		);
	}

	public void LogMainMenuProfilePageRestorePurchasesClick()
	{
		LogEvent("main_menu_profile_page_restore_purchases_click");
	}

	public void LogMainMenuProfilePageSignInClick(string _ProviderID)
	{
		LogEvent(
			"main_menu_profile_page_sign_in_click",
			StatisticData.Create("provider_id", _ProviderID)
		);
	}

	public void LogMainMenuProfilePageSignOutClick(string _ProviderID)
	{
		LogEvent(
			"main_menu_profile_page_sign_out_click",
			StatisticData.Create("provider_id", _ProviderID)
		);
	}

	#endregion

	#region Level Menu

	public void LogSongMenuUnlockClick(string _SongID)
	{
		LogEvent(
			"song_menu_unlock_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogSongMenuUnlockSuccess(string _LevelID)
	{
		LogEvent(
			"song_menu_unlock_success",
			StatisticData.Create("song_id", _LevelID)
		);
	}

	public void LogSongMenuUnlockFailed(string _LevelID)
	{
		LogEvent(
			"level_menu_unlock_failed",
			StatisticData.Create("level_id", _LevelID)
		);
	}

	public void LogSongMenuPlayClick(string _LevelID)
	{
		LogEvent(
			"level_menu_play_click",
			StatisticData.Create("level_id", _LevelID)
		);
	}

	public void LogSongMenuNextClick(string _LevelID)
	{
		LogEvent(
			"level_menu_next_click",
			StatisticData.Create("level_id", _LevelID)
		);
	}

	public void LogSongMenuPreviousClick(string _LevelID)
	{
		LogEvent(
			"level_menu_previous_click",
			StatisticData.Create("level_id", _LevelID)
		);
	}

	#endregion

	#region Product Menu

	public void LogPurchase(string _ProductID, string _Currency, decimal _Price)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Purchase(_ProductID, _Currency, _Price);
	}

	public void LogProductMenuPurchaseClick(string _ProductID)
	{
		LogEvent(
			"product_menu_purchase_click",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogProductMenuPurchaseSuccess(string _ProductID)
	{
		LogEvent(
			"product_menu_purchase_success",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogProductMenuPurchaseFailed(string _ProductID)
	{
		LogEvent(
			"product_menu_purchase_failed",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogProductMenuNextClick(string _ProductID)
	{
		LogEvent(
			"product_menu_next_click",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	public void LogProductMenuPreviousClick(string _ProductID)
	{
		LogEvent(
			"product_menu_previous_click",
			StatisticData.Create("product_id", _ProductID)
		);
	}

	#endregion

	#region Pause Menu

	public void LogPauseMenuLeaveClick(string _SongID)
	{
		LogEvent(
			"pause_menu_leave_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogPauseMenuRestartClick(string _SongID)
	{
		LogEvent(
			"pause_menu_restart_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogPauseMenuLatencyClick(string _SongID)
	{
		LogEvent(
			"pause_menu_latency_click",
			StatisticData.Create("level_id", _SongID)
		);
	}

	public void LogPauseMenuHaptic(bool _State)
	{
		LogEvent(
			"pause_menu_haptic_state",
			StatisticData.Create("state", _State)
		);
	}

	#endregion

	#region Result Menu

	public void LogResultMenuRewardPageContinueClick(string _SongID)
	{
		LogEvent(
			"result_menu_reward_page_continue_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogResultMenuLevelPageContinueClick(string _SongID)
	{
		LogEvent(
			"result_menu_level_page_continue_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogResultMenuControlPageLeaveClick(string _SongID)
	{
		LogEvent(
			"result_menu_control_page_leave_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogResultMenuControlPageNextClick(string _SongID)
	{
		LogEvent(
			"result_menu_control_page_next_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogResultMenuControlPageRestartClick(string _SongID)
	{
		LogEvent(
			"result_menu_control_page_restart_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogResultMenuControlPagePlatformClick(string _SongID, string _PlatformURL)
	{
		LogEvent(
			"result_menu_control_page_platform_click",
			StatisticData.Create("song_id", _SongID),
			StatisticData.Create("platform_url", _PlatformURL)
		);
	}

	#endregion

	#region Revive Menu

	public void LogReviveMenuShow(string _SongID)
	{
		LogEvent(
			"revive_menu_show",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogReviveMenuReviveAdsClick(string _SongID)
	{
		LogEvent(
			"revive_menu_revive_ads_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogReviveMenuReviveCoinsClick(string _SongID)
	{
		LogEvent(
			"revive_menu_revive_coins_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogReviveMenuLeaveClick(string _SongID)
	{
		LogEvent(
			"revive_menu_leave_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	public void LogReviveMenuRestartClick(string _SongID)
	{
		LogEvent(
			"revive_menu_restart_click",
			StatisticData.Create("song_id", _SongID)
		);
	}

	#endregion

	#region Latency Menu

	public void LogAudioLatencyState(string _DeviceName, string _DeviceUID, AudioOutputType _DeviceType, float _Latency)
	{
		LogEvent(
			"latency_menu_state",
			StatisticData.Create("device_name", _DeviceName),
			StatisticData.Create("device_uid", _DeviceUID),
			StatisticData.Create("device_type", _DeviceType.ToString()),
			StatisticData.Create("latency", _Latency)
		);
	}

	#endregion

	#region Error Menu

	public void LogErrorMenuShow(string _Reason)
	{
		LogEvent(
			"error_menu_show",
			StatisticData.Create("reason", _Reason)
		);
	}

	#endregion

	#region Retry Menu

	public void LogRetryMenuShow(string _Reason)
	{
		LogEvent(
			"retry_menu_show",
			StatisticData.Create("reason", _Reason)
		);
	}

	public void LogRetryMenuRetryClick(string _Reason)
	{
		LogEvent(
			"retry_menu_retry_click",
			StatisticData.Create("reason", _Reason)
		);
	}

	public void LogRetryMenuCancelClick(string _Reason)
	{
		LogEvent(
			"retry_menu_cancel_click",
			StatisticData.Create("reason", _Reason)
		);
	}

	#endregion

	#region Banner Menu

	public void LogBannerMenuCloseClick(string _BannerID)
	{
		LogEvent(
			"banner_menu_close_click",
			StatisticData.Create("banner_id", _BannerID)
		);
	}

	public void LogBannerMenuOpenClick(string _BannerID)
	{
		LogEvent(
			"banner_menu_open_click",
			StatisticData.Create("banner_id", _BannerID)
		);
	}

	#endregion

	void LogEvent(string _Name, params StatisticData[] _Parameters)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Log(_Name, _Parameters);
	}
}
