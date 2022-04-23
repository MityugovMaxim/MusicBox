using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class StringExtension
{
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
}