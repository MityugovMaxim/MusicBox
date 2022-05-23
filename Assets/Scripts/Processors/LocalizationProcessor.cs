using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LocalizationProcessor
{
	[Inject] StorageProcessor m_StorageProcessor;

	readonly Dictionary<string, string> m_Localization = new Dictionary<string, string>();

	string m_Language;

	CancellationTokenSource m_TokenSource;

	public async Task Load(string _Language)
	{
		if (m_Language == _Language)
			return;
		
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		m_Language = _Language;
		
		m_Localization.Clear();
		
		string json = null;
		try
		{
			json = await m_StorageProcessor.LoadJson(
				$"Localization/{m_Language}.lang",
				true,
				Encoding.Unicode,
				null,
				token
			);
		}
		catch (TaskCanceledException)
		{
			Debug.LogFormat("[LocalizationProcessor] Load localization canceled. Language: {0}.", m_Language);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogErrorFormat("[LocalizationProcessor] Load localization failed. Language: {0}.", m_Language);
		}
		
		if (token.IsCancellationRequested)
			return;
		
		if (Json.Deserialize(json) is Dictionary<string, object> data)
		{
			foreach (string key in data.Keys)
				m_Localization[key] = data.GetString(key);
		}
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	public string Format(string _Key, object _Arg)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] FORMAT: INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format))
			return $"[{m_Language}] FORMAT: MISSING {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] FORMAT: NULL {_Key}";
		
		try
		{
			return string.Format(format, _Arg);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] FORMAT: ERROR {_Key}";
	}

	public string Format(string _Key, object _Arg0, object _Arg1)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] FORMAT: INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format))
			return $"[{m_Language}] FORMAT: MISSING {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] FORMAT: NULL {_Key}";
		
		try
		{
			return string.Format(format, _Arg0, _Arg1);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] FORMAT: ERROR {_Key}";
	}

	public string Format(string _Key, object _Arg0, object _Arg1, object _Arg2)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] FORMAT: INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format))
			return $"[{m_Language}] FORMAT: MISSING {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] FORMAT: NULL {_Key}";
		
		try
		{
			return string.Format(format, _Arg0, _Arg1, _Arg2);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] FORMAT: ERROR {_Key}";
	}

	public string Format(string _Key, params object[] _Args)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] FORMAT: INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format))
			return $"[{m_Language}] FORMAT: MISSING {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] FORMAT: NULL {_Key}";
		
		try
		{
			return string.Format(format, _Args);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] FORMAT: ERROR {_Key}";
	}

	public string Get(string _Key, string _Default = null)
	{
		if (string.IsNullOrEmpty(_Key))
			return _Default ?? $"[{m_Language}] GET: INVALID";
		
		if (m_Localization.TryGetValue(_Key, out string value))
			return value ?? $"[{m_Language}] GET: NULL";
		
		return _Default ?? (string.IsNullOrEmpty(_Key) ? $"[{m_Language}] GET: MISSING NULL" : $"[{m_Language}] GET: MISSING {_Key}");
	}
}