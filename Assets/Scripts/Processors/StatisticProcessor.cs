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
}

[Preserve]
public class StatisticAppMetrica : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			"am_purchase",
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
}

[Preserve]
public class StatisticAppsFlyer : IStatisticProvider
{
	public void Purchase(string _ProductID, string _Currency, decimal _Price)
	{
		Log(
			AFInAppEvents.PURCHASE,
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
}

[Preserve]
public class StatisticProcessor
{
	[Inject] IStatisticProvider[] m_Providers;

	public void LogPurchase(string _ProductID, string _Currency, decimal _Price)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Purchase(_ProductID, _Currency, _Price);
	}

	public void LogEvent(string _Name, params StatisticData[] _Parameters)
	{
		if (m_Providers == null || m_Providers.Length == 0)
			return;
		
		foreach (IStatisticProvider provider in m_Providers)
			provider.Log(_Name, _Parameters);
	}
}
