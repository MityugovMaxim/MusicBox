using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExtension
{
	static readonly string[] m_Words =
	{
		"iOS",
		"IDs",
		"ID",
		"BPM",
		"ASF",
		"URL",
	};

	public static int NaturalCompareTo(this string _Source, string _Target)
	{
		MatchCollection source = Regex.Matches(_Source, @"\d+");
		MatchCollection target = Regex.Matches(_Target, @"\d+");
		
		int max = Mathf.Max(
			source.Count > 0 ? source.Max(_Match => _Match.Value.Length) : 0,
			target.Count > 0 ? target.Max(_Match => _Match.Value.Length) : 0
		);
		
		string a = Regex.Replace(_Source, @"\d+", _Match => _Match.Value.PadLeft(max, '0'));
		string b = Regex.Replace(_Target, @"\d+", _Match => _Match.Value.PadLeft(max, '0'));
		
		return string.Compare(a, b, StringComparison.Ordinal);
	}

	public static string ToDisplayName(this string _String)
	{
		if (string.IsNullOrEmpty(_String))
			return _String;
		
		string[] data = _String.Split(m_Words, StringSplitOptions.RemoveEmptyEntries);
		
		string result = _String;
		
		foreach (string entry in data)
		{
			string target = entry.SplitWords();
			
			result = result.Replace(entry, target);
		}
		
		return result.Trim();
	}

	static string SplitWords(this string _String)
	{
		if (string.IsNullOrEmpty(_String))
			return _String;
		
		StringBuilder builder = new StringBuilder();
		List<string>  words   = new List<string>();
		
		builder.Append(' ');
		
		for (int i = 1; i < _String.Length; i++)
		{
			char source = _String[i - 1];
			char target = _String[i];
			
			if (!char.IsLetterOrDigit(source))
			{
				if (builder.Length > 0)
				{
					words.Add(builder.ToString());
					builder.Clear();
				}
				continue;
			}
			
			builder.Append(source);
			
			if (char.IsLetter(source) ^ char.IsLetter(target))
			{
				words.Add(builder.ToString());
				builder.Clear();
			}
			else if (char.IsLower(source) && char.IsUpper(target))
			{
				words.Add(builder.ToString());
				builder.Clear();
			}
		}
		builder.Append(_String[^1]);
		words.Add(builder.ToString());
		builder.Clear();
		
		foreach (string word in words)
		{
			builder.Append(word.ToCapital());
			builder.Append(' ');
		}
		
		return builder.ToString();
	}

	public static string ToCapital(this string _String)
	{
		StringBuilder builder = new StringBuilder();
		bool          first   = true;
		foreach (char symbol in _String)
		{
			if (char.IsLetter(symbol))
			{
				builder.Append(first ? char.ToUpperInvariant(symbol) : char.ToLowerInvariant(symbol));
				first = false;
			}
			else
			{
				builder.Append(symbol);
			}
		}
		return builder.ToString();
	}

	public static string ToAllCapital(this string _String)
	{
		if (string.IsNullOrEmpty(_String))
			return _String;
		
		StringBuilder builder = new StringBuilder();
		
		List<string> words = new List<string>();
		
		foreach (char symbol in _String)
		{
			if (char.IsLetterOrDigit(symbol))
			{
				builder.Append(symbol);
			}
			else if (builder.Length > 0)
			{
				words.Add(builder.ToString());
				builder.Clear();
			}
		}
		
		words.Add(builder.ToString());
		builder.Clear();
		
		return string.Join("_", words.Select(_Word => _Word.ToUpperInvariant()));
	}

	public static (int Major, int Minor, int Patch) ParseVersion(this string _String)
	{
		if (string.IsNullOrEmpty(_String))
			return (0, 0, 0);
		
		string[] data = _String.Split(new char[] { '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
		
		int major;
		if (data.Length >= 1 && !string.IsNullOrEmpty(data[0]))
			int.TryParse(data[0], out major);
		else
			major = 0;
		
		int minor;
		if (data.Length >= 2 && !string.IsNullOrEmpty(data[1]))
			int.TryParse(data[1], out minor);
		else
			minor = 0;
		
		int patch;
		if (data.Length >= 3 && !string.IsNullOrEmpty(data[2]))
			int.TryParse(data[1], out patch);
		else
			patch = 0;
		
		return (major, minor, patch);
	}

	public static string GenerateUniqueID<T>(this ICollection<T> _List, string _ID, Func<T, string> _Selector)
	{
		if (_List == null || _List.Count == 0 || _Selector == null)
			return _ID;
			
		HashSet<string> ids = new HashSet<string>();
		foreach (string id in _List.Select(_Selector))
		{
			if (!string.IsNullOrEmpty(id))
				ids.Add(id);
		}
		
		const int limit = 250;
		
		for (int i = 1; i <= limit; i++)
		{
			string uniqueID = $"{_ID} {i}";
			if (ids.Contains(uniqueID))
				continue;
			return uniqueID;
		}
		
		return $"{_ID} {CRC32.Get(Guid.NewGuid().ToString())}";
	}
}