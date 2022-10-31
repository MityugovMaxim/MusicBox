using System.Collections.Generic;
using System.Globalization;
using AppsFlyerSDK;
using Facebook.Unity;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Scripting;
using Zenject;

public abstract class StatisticData
{
	public abstract Parameter GetParameter();
	public abstract void Fill(Dictionary<string, object> _Data);
	public abstract void Fill(Dictionary<string, string> _Data);
	protected abstract string GetString();

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

	public static StatisticData Create(string _Key, decimal _Value)
	{
		return new StatisticDecimal(_Key, _Value);
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
		_Data[Key] = GetString();
	}
}

public class StatisticString : StatisticData<string>
{
	public StatisticString(string _Key, string _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, GetString());
	}

	protected override string GetString() => Value ?? "null";
}

public class StatisticInteger : StatisticData<int>
{
	public StatisticInteger(string _Key, int _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}

	protected override string GetString() => Value.ToString();
}

public class StatisticLong : StatisticData<long>
{
	public StatisticLong(string _Key, long _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}

	protected override string GetString() => Value.ToString();
}

public class StatisticDouble : StatisticData<double>
{
	public StatisticDouble(string _Key, double _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value);
	}

	protected override string GetString() => Value.ToString(CultureInfo.InvariantCulture);
}

public class StatisticDecimal : StatisticData<decimal>
{
	public StatisticDecimal(string _Key, decimal _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, (double)Value);
	}

	protected override string GetString() => Value.ToString(CultureInfo.InvariantCulture);
}

public class StatisticBool : StatisticData<bool>
{
	public StatisticBool(string _Key, bool _Value) : base(_Key, _Value) { }

	public override Parameter GetParameter()
	{
		return new Parameter(Key, Value ? 1 : 0);
	}

	protected override string GetString() => Value.ToString();
}

public interface IStatisticProvider
{
	void Purchase(string _ProductID, string _Currency, decimal _Price);

	void Log(string _Name, params StatisticData[] _Parameters);

	void LogImmediate(string _Name, params StatisticData[] _Parameters);
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

	public void LogImmediate(string _Name, params StatisticData[] _Parameters)
	{
		Log(_Name, _Parameters);
		
		Analytics.FlushEvents();
	}
}

[Preserve]
public class StatisticFacebook : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			"payment_succeed_fb",
			StatisticData.Create("product_id", _ProductID),
			StatisticData.Create("price", (double)_Price),
			StatisticData.Create("currency", _Currency)
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

	public void LogImmediate(string _Name, params StatisticData[] _Parameters)
	{
		Log(_Name, _Parameters);
	}
}

[Preserve]
public class StatisticFirebase : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			FirebaseAnalytics.EventPurchase,
			StatisticData.Create("product_id", _ProductID),
			StatisticData.Create("price", (double)_Price),
			StatisticData.Create("currency", _Currency)
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

	public void LogImmediate(string _Name, params StatisticData[] _Parameters)
	{
		Log(_Name, _Parameters);
	}
}

[Preserve]
public class StatisticAppMetrica : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			"payment_succeed_appm",
			StatisticData.Create("product_id", _ProductID),
			StatisticData.Create("price", (double)_Price),
			StatisticData.Create("currency", _Currency)
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
		
		AppMetrica.Instance.ReportEvent(_Name, data);
	}

	public void LogImmediate(string _Name, params StatisticData[] _Parameters)
	{
		Log(_Name, _Parameters);
		
		AppMetrica.Instance.SendEventsBuffer();
	}
}

[Preserve]
public class StatisticAppsFlyer : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			"payment_succeed_af",
			StatisticData.Create("product_id", _ProductID),
			StatisticData.Create(AFInAppEvents.CURRENCY, _Currency),
			StatisticData.Create(AFInAppEvents.REVENUE, (double)_Price),
			StatisticData.Create(AFInAppEvents.QUANTITY, 1)
		);
	}

	public void Log(string _Name, params StatisticData[] _Parameters)
	{
		Dictionary<string, string> data = new Dictionary<string, string>();
		
		if (_Parameters != null && _Parameters.Length > 0)
		{
			foreach (StatisticData parameter in _Parameters)
				parameter.Fill(data);
		}
		
		AppsFlyer.sendEvent(_Name, data);
	}

	public void LogImmediate(string _Name, params StatisticData[] _Parameters)
	{
		Log(_Name, _Parameters);
	}
}

public enum TechnicalStepType
{
	Launch,
	Login,
	TutorialStart,
	TutorialFinish,
	TutorialSkip,
	SongStart,
	SongFinish,
	SongRestart,
	SongLose,
	SongLeave,
	SongNext,
	SongUnlock,
}

[Preserve]
public class StatisticProcessor
{
	const string SONG_START_COUNT_KEY = "SONG_START_COUNT";

	static int SongStartCount
	{
		get => PlayerPrefs.GetInt(SONG_START_COUNT_KEY, 0);
		set => PlayerPrefs.SetInt(SONG_START_COUNT_KEY, value);
	}

	static int TechnicalStep { get; set; }

	[Inject] IStatisticProvider[] m_Providers;

	public void LogAdsAvailable(string _Type, string _PlacementID, bool _Available)
	{
		LogEvent(
			"video_ads_available",
			StatisticData.Create("ad_type", _Type),
			StatisticData.Create("placement", _PlacementID),
			StatisticData.Create("result", _Available ? "success" : "not_available"),
			StatisticData.Create("connection", Application.internetReachability != NetworkReachability.NotReachable)
		);
	}

	public void LogAdsStarted(string _Type, string _PlacementID)
	{
		LogEvent(
			"video_ads_started",
			StatisticData.Create("ad_type", _Type),
			StatisticData.Create("placement", _PlacementID),
			StatisticData.Create("result", "start"),
			StatisticData.Create("connection", Application.internetReachability != NetworkReachability.NotReachable)
		);
	}

	public void LogAdsFinished(string _Type, string _PlacementID, string _Result)
	{
		LogEvent(
			"video_ads_watch",
			StatisticData.Create("ad_type", _Type),
			StatisticData.Create("placement", _PlacementID),
			StatisticData.Create("result", _Result),
			StatisticData.Create("connection", Application.internetReachability != NetworkReachability.NotReachable)
		);
	}

	public void LogLink(string _Type, string _URL)
	{
		LogEvent(
			"link",
			StatisticData.Create("type", _Type),
			StatisticData.Create("url", _URL)
		);
	}

	public void LogSongStart(
		string _SongID,
		int    _SongNumber,
		int    _SongLevel,
		string _SongType
	)
	{
		SongStartCount++;
		
		LogEventImmediate(
			"level_start",
			StatisticData.Create("level_number", _SongNumber),
			StatisticData.Create("level_name", _SongID),
			StatisticData.Create("level_count", SongStartCount),
			StatisticData.Create("level_diff", _SongLevel),
			StatisticData.Create("level_type", _SongType),
			StatisticData.Create("level_random", false)
		);
	}

	public void LogSongFinish(
		string _SongID,
		int    _SongNumber,
		int    _SongLevel,
		long   _Price,
		int    _Progress,
		int    _Revives,
		int    _Time,
		string _Result
	)
	{
		LogEventImmediate(
			"level_finish",
			StatisticData.Create("level_name", _SongID),
			StatisticData.Create("level_number", _SongNumber),
			StatisticData.Create("level_count", SongStartCount),
			StatisticData.Create("level_type", _Price > 0 ? "paid" : "free"),
			StatisticData.Create("level_diff", _SongLevel),
			StatisticData.Create("level_random", false),
			StatisticData.Create("progress", _Progress),
			StatisticData.Create("continue", _Revives),
			StatisticData.Create("time", _Time),
			StatisticData.Create("result", _Result)
		);
	}

	public void LogTutorial(int _Step, string _Name)
	{
		LogEventImmediate(
			"tutorial",
			StatisticData.Create("step_name", $"{_Step:00}_{_Name}")
		);
	}

	public void LogLevelUp(int _Level)
	{
		LogEvent(
			"level_up",
			StatisticData.Create("level", _Level)
		);
	}

	public void LogError(string _Type, string _Place)
	{
		LogEventImmediate(
			"errors",
			StatisticData.Create("type", _Type),
			StatisticData.Create("place", _Place)
		);
	}

	public void LogTechnicalStep(TechnicalStepType _Type)
	{
		string key = $"TECHNICAL_STEP_{_Type}";
		
		bool first = !PlayerPrefs.HasKey(key);
		
		PlayerPrefs.SetInt(key, 1);
		
		TechnicalStep++;
		
		LogEventImmediate(
			"technical",
			StatisticData.Create("step_name", $"{TechnicalStep:00}_{_Type}"),
			StatisticData.Create("first", first)
		);
	}

	public void LogPurchase(string _ProductID, string _Currency, decimal _Price)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Purchase(_ProductID, _Currency, _Price);
	}

	public void LogLogin(string _UserID, string _Name)
	{
		LogEvent(
			"login",
			StatisticData.Create("user_id", _UserID),
			StatisticData.Create("name", _Name)
		);
	}

	public void LogEvent(string _Name, params StatisticData[] _Parameters)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Log(_Name, _Parameters);
	}

	public void LogEventImmediate(string _Name, params StatisticData[] _Parameters)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.LogImmediate(_Name, _Parameters);
	}
}
