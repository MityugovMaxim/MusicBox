using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.MiniJSON;

public class LocalizationData
{
	public class Entry
	{
		public string Key   { get; set; }
		public string Value { get; set; }

		public Entry()
		{
			Key   = string.Empty;
			Value = string.Empty;
		}

		public Entry(string _Key)
		{
			Key   = _Key ?? string.Empty;
			Value = string.Empty;
		}

		public Entry(string _Key, string _Value)
		{
			Key   = _Key ?? string.Empty;
			Value = _Value ?? string.Empty;
		}
	}

	public string                              Language     { get; }
	public IReadOnlyDictionary<string, string> Localization => m_Localization;
	public IReadOnlyList<Entry>                Entries      => m_Entries;

	public event Action OnChanged;

	readonly Dictionary<string, string> m_Localization;
	readonly List<Entry>                m_Entries;

	public LocalizationData(string _Language, Dictionary<string, string> _Localization)
	{
		Language       = _Language;
		m_Localization = _Localization ?? new Dictionary<string, string>();
		
		m_Entries = new List<Entry>();
		foreach (var entry in Localization)
			m_Entries.Add(new Entry(entry.Key, entry.Value));
	}

	public string Serialize()
	{
		Dictionary<string, string> data = new Dictionary<string, string>();
		foreach (Entry entry in m_Entries)
			data[entry.Key] = entry.Value;
		return Json.Serialize(data);
	}

	public void Rebuild(Dictionary<string, string> _Localization)
	{
		if (_Localization == null)
			return;
		
		m_Localization.Clear();
		m_Entries.Clear();
		
		foreach (var entry in _Localization)
		{
			m_Localization[entry.Key] = entry.Value;
			m_Entries.Add(new Entry(entry.Key, entry.Value));
		}
		
		OnChanged?.Invoke();
	}

	public void Reorder(int _SourceIndex, int _TargetIndex)
	{
		if (_SourceIndex == _TargetIndex)
			return;
		
		if (_SourceIndex < 0 || _TargetIndex < 0)
			return;
		
		Entry entry = m_Entries[_SourceIndex];
		
		m_Entries.RemoveAt(_SourceIndex);
		m_Entries.Insert(_TargetIndex, entry);
		
		OnChanged?.Invoke();
	}

	public void Sort()
	{
		m_Entries.Sort((_A, _B) => _A.Key.NaturalCompareTo(_B.Key));
		
		OnChanged?.Invoke();
	}

	public void Rename(string _SourceKey, string _TargetKey)
	{
		if (string.IsNullOrEmpty(_SourceKey) || string.IsNullOrEmpty(_TargetKey))
			return;
		
		if (_SourceKey == _TargetKey)
			return;
		
		if (!m_Localization.ContainsKey(_SourceKey) || m_Localization.ContainsKey(_TargetKey))
			return;
		
		string value = GetValue(_SourceKey);
		
		m_Localization.Remove(_SourceKey);
		m_Localization[_TargetKey] = value;
		
		Entry entry = m_Entries.FirstOrDefault(_Entry => _Entry.Key == _SourceKey);
		if (entry != null)
			entry.Key = _TargetKey;
		else
			m_Entries.Add(new Entry(_SourceKey, value));
		
		OnChanged?.Invoke();
	}

	public void Add(string _Key, string _Value)
	{
		if (string.IsNullOrEmpty(_Key))
			return;
		
		if (m_Localization == null || m_Localization.ContainsKey(_Key))
			return;
		
		m_Localization[_Key] = _Value;
		m_Entries.Add(new Entry(_Key, _Value));
		
		OnChanged?.Invoke();
	}

	public void Create()
	{
		const string key = "NEW_KEY";
		
		if (!m_Localization.ContainsKey(key))
		{
			m_Localization[key] = string.Empty;
			m_Entries.Add(new Entry(key));
			return;
		}
		
		const int attempts = 100;
		for (int i = 1; i <= attempts; i++)
		{
			string numeric = key + $"_{i:00}";
			
			if (m_Localization.ContainsKey(numeric))
				continue;
			
			m_Localization[numeric] = string.Empty;
			m_Entries.Add(new Entry(numeric));
			
			return;
		}
		
		string guid = $"{key}_{Guid.NewGuid()}";
		
		m_Localization[guid] = string.Empty;
		m_Entries.Add(new Entry(guid));
	}

	public void Remove(string _Key)
	{
		if (string.IsNullOrEmpty(_Key))
			return;
		
		if (m_Localization == null || !m_Localization.ContainsKey(_Key))
			return;
		
		m_Localization.Remove(_Key);
		
		m_Entries.RemoveAll(_Entry => _Entry.Key == _Key);
		
		OnChanged?.Invoke();
	}

	public void SetValue(string _Key, string _Value)
	{
		if (string.IsNullOrEmpty(_Key))
			return;
		
		if (m_Localization == null || !m_Localization.ContainsKey(_Key))
			return;
		
		m_Localization[_Key] = _Value;
		
		Entry entry = m_Entries.FirstOrDefault(_Entry => _Entry.Key == _Key);
		
		if (entry != null)
			entry.Value = _Value;
		else
			m_Entries.Add(new Entry(_Key, _Value));
		
		OnChanged?.Invoke();
	}

	public string GetValue(string _Key)
	{
		if (string.IsNullOrEmpty(_Key))
			return null;
		
		if (Localization == null)
			return null;
		
		return Localization.TryGetValue(_Key, out string value) ? value : null;
	}
}