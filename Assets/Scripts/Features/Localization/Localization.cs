using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class Localization
{
	public bool Loaded { get; private set; }

	[Inject] LocalizationProvider m_LocalizationProvider;

	readonly Dictionary<string, string> m_BuiltIn      = new Dictionary<string, string>();
	readonly Dictionary<string, string> m_Localization = new Dictionary<string, string>();

	string m_Language;

	Task m_Loading;

	public async Task Load(string _Language)
	{
		if (Loaded && m_Language == _Language)
			return;
		
		if (m_Loading != null)
		{
			await m_Loading;
			return;
		}
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_Loading = completionSource.Task;
		
		LocalizationRegistry.Load(m_BuiltIn);
		
		m_Language = _Language;
		
		m_Localization.Clear();
		
		Loaded = await Fetch();
		
		completionSource.TrySetResult(true);
		
		m_Loading = null;
	}

	public Task Reload(string _Language)
	{
		m_Language = null;
		
		return Load(_Language);
	}

	public void Unload()
	{
		Loaded = false;
		
		m_Language = null;
		
		m_Localization.Clear();
	}

	public string Format(string _Key, object _Arg)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format) && !m_BuiltIn.TryGetValue(_Key, out format))
			return $"[{m_Language}] {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] {_Key}";
		
		try
		{
			return string.Format(format, _Arg);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] ERROR {_Key}";
	}

	public string Format(string _Key, object _Arg0, object _Arg1)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format) && !m_BuiltIn.TryGetValue(_Key, out format))
			return $"[{m_Language}] {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] {_Key}";
		
		try
		{
			return string.Format(format, _Arg0, _Arg1);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] ERROR {_Key}";
	}

	public string Format(string _Key, object _Arg0, object _Arg1, object _Arg2)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format) && !m_BuiltIn.TryGetValue(_Key, out format))
			return $"[{m_Language}] {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] {_Key}";
		
		try
		{
			return string.Format(format, _Arg0, _Arg1, _Arg2);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] ERROR {_Key}";
	}

	public string Format(string _Key, params object[] _Args)
	{
		if (string.IsNullOrEmpty(_Key))
			return $"[{m_Language}] INVALID";
		
		if (!m_Localization.TryGetValue(_Key, out string format) && !m_BuiltIn.TryGetValue(_Key, out format))
			return $"[{m_Language}] {_Key}";
		
		if (string.IsNullOrEmpty(format))
			return $"[{m_Language}] {_Key}";
		
		try
		{
			return string.Format(format, _Args);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		
		return $"[{m_Language}] ERROR {_Key}";
	}

	public string Get(string _Key, string _Default = null)
	{
		if (string.IsNullOrEmpty(_Key))
			return _Default ?? $"[{m_Language}] INVALID";
		
		if (m_Localization.TryGetValue(_Key, out string value))
			return value ?? $"[{m_Language}] NULL";
		
		if (m_BuiltIn.TryGetValue(_Key, out value))
			return value ?? $"[{m_Language}] NULL";
		
		return _Default ?? (string.IsNullOrEmpty(_Key) ? $"[{m_Language}] NULL" : $"[{m_Language}] {_Key}");
	}

	async Task<bool> Fetch()
	{
		m_Localization.Clear();
		
		try
		{
			Dictionary<string, string> localization = await m_LocalizationProvider.DownloadAsync($"Localization/{m_Language}.lang");
			
			foreach (var entry in localization)
				m_Localization[entry.Key] = entry.Value;
			
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			return false;
		}
	}
}
