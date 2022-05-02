using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class StringExtension
{
	public static string ToUnique(this string _String, char _Separator, ICollection<string> _Values)
	{
		return _Values != null ? _String.ToUnique(_Separator, _Values.Contains) : _String;
	}

	public static string ToUnique(this string _String, char _Separator, Func<string, bool> _Selector)
	{
		if (_Selector == null)
			return _String;
		
		for (int i = 1; i <= 99; i++)
		{
			string value = $"{_String}{_Separator}{i:00}";
			
			if (_Selector(value))
				continue;
			
			return value;
		}
		
		return $"{_String}{_Separator}{Guid.NewGuid().ToString().Substring(0, 8)}";
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

	public static string GetDisplayName(this string _String)
	{
		string[] words = _String.GetWords();
		for (int i = 0; i < words.Length; i++)
			words[i] = words[i].Capitalize();
		return string.Join(" ", words);
	}

	public static string Capitalize(this string _String)
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _String.Length; i++)
		{
			char symbol = i == 0
				? char.ToUpperInvariant(_String[i])
				: char.ToLowerInvariant(_String[i]);
			
			builder.Append(symbol);
		}
		return builder.ToString();
	}

	public static string[] GetWords(this string _String)
	{
		StringBuilder builder = new StringBuilder();
		
		List<string> words = new List<string>();
		
		bool lowerCase = char.IsLower(_String[0]);
		
		// DoubleValue
		
		foreach (char symbol in _String)
		{
			if (char.IsLetterOrDigit(symbol))
			{
				if (lowerCase && char.IsUpper(symbol))
				{
					lowerCase = false;
					words.Add(builder.ToString());
					builder.Clear();
				}
				lowerCase = char.IsLower(symbol);
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
		
		return words.ToArray();
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
}