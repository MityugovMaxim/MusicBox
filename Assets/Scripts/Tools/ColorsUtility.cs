using UnityEngine;

public static class ColorsUtility
{
	public static string GetHTML(Color _Color)
	{
		Color32 color = _Color;
		
		string r = color.r.ToString("X2");
		string g = color.g.ToString("X2");
		string b = color.b.ToString("X2");
		string a = color.a.ToString("X2");
		
		return $"#{r}{g}{b}{a}";
	}

	public static Color ToColor(this string _HTML)
	{
		if (string.IsNullOrEmpty(_HTML))
			return Color.clear;
		
		string html = _HTML.StartsWith('#') ? _HTML[1..] : _HTML;
		
		byte r = html.Length >= 2 ? GetByte(html[0], html[1]) : byte.MaxValue;
		byte g = html.Length >= 4 ? GetByte(html[2], html[3]) : byte.MaxValue;
		byte b = html.Length >= 6 ? GetByte(html[4], html[5]) : byte.MaxValue;
		byte a = html.Length >= 8 ? GetByte(html[6], html[7]) : byte.MaxValue;
		
		return new Color32(r, g, b, a);
	}

	static byte GetByte(char _Symbol)
	{
		const int offset    = 'A';
		const int threshold = '0';
		
		int symbol = _Symbol;
		
		int value = symbol < offset ? symbol - threshold : 10 + (symbol - offset);
		
		return (byte)value;
	}

	static byte GetByte(char _A, char _B)
	{
		byte a     = GetByte(_A);
		byte b     = GetByte(_B);
		byte value = (byte)((a << 4) + b);
		return value;
	}
}