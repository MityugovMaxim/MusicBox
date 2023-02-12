using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;

public static class DataSnapshotExtension
{
	public static TValue GetValue<TValue>(this DataSnapshot _DataSnapshot, TValue _Default = default)
	{
		Type type = typeof(TValue);
		
		if (type == typeof(string))
			return (TValue)(object)_DataSnapshot.GetString();
		if (type == typeof(int))
			return (TValue)(object)_DataSnapshot.GetInt();
		if (type == typeof(float))
			return (TValue)(object)_DataSnapshot.GetFloat();
		if (type == typeof(long))
			return (TValue)(object)_DataSnapshot.GetLong();
		if (type == typeof(double))
			return (TValue)(object)_DataSnapshot.GetDouble();
		if (type == typeof(bool))
			return (TValue)(object)_DataSnapshot.GetBool();
		if (type == typeof(Enum))
			return (TValue)(object)_DataSnapshot.GetEnum(type);
		
		return _Default;
	}

	public static string GetString(this DataSnapshot _DataSnapshot)
	{
		return (string)_DataSnapshot.Value;
	}

	public static string GetString(this DataSnapshot _DataSnapshot, string _Name, string _Default = null)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetString();
		return _Default;
	}

	public static Color GetColor(this DataSnapshot _DataSnapshot, string _Name, string _Default = "#FFFFFFFF")
	{
		string color = _DataSnapshot.GetString(_Name, _Default);
		
		return ColorUtility.TryParseHtmlString(color, out Color value) ? value : Color.white;
	}

	public static int GetInt(this DataSnapshot _DataSnapshot)
	{
		return int.Parse(_DataSnapshot.GetRawJsonValue());
	}

	public static int GetInt(this DataSnapshot _DataSnapshot, string _Name, int _Default = 0)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetInt();
		return _Default;
	}

	public static bool GetBool(this DataSnapshot _DataSnapshot)
	{
		return (bool)_DataSnapshot.Value;
	}

	public static bool GetBool(this DataSnapshot _DataSnapshot, string _Name, bool _Default = false)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetBool();
		return _Default;
	}

	public static float GetFloat(this DataSnapshot _DataSnapshot)
	{
		return Convert.ToSingle(_DataSnapshot.Value);
	}

	public static float GetFloat(this DataSnapshot _DataSnapshot, string _Name, float _Default = 0)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetFloat();
		return _Default;
	}

	public static double GetDouble(this DataSnapshot _DataSnapshot)
	{
		return Convert.ToDouble(_DataSnapshot.Value);
	}

	public static double GetDouble(this DataSnapshot _DataSnapshot, string _Name, double _Default = 0)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetDouble();
		return _Default;
	}

	public static long GetLong(this DataSnapshot _DataSnapshot)
	{
		return _DataSnapshot.Value != null ? (long)_DataSnapshot.Value : default;
	}

	public static long GetLong(this DataSnapshot _DataSnapshot, string _Name, long _Default = 0)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetLong();
		return _Default;
	}

	public static T GetEnum<T>(this DataSnapshot _DataSnapshot) where T : Enum
	{
		return (T)Enum.Parse(typeof(T), _DataSnapshot.GetRawJsonValue());
	}

	public static Enum GetEnum(this DataSnapshot _DataSnapshot, Type _Type)
	{
		return (Enum)Enum.Parse(_Type, _DataSnapshot.GetRawJsonValue());
	}

	public static T GetEnum<T>(this DataSnapshot _DataSnapshot, string _Name, T _Default = default) where T : Enum
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetEnum<T>();
		return _Default;
	}

	public static List<string> GetChildKeys(this DataSnapshot _DataSnapshot)
	{
		return _DataSnapshot.Children
			.Select(_Entry => _Entry.Key)
			.ToList();
	}

	public static List<string> GetChildKeys(this DataSnapshot _DataSnapshot, string _Name, List<string> _Default = null)
	{
		if (_DataSnapshot.HasChild(_Name))
			return _DataSnapshot.Child(_Name).GetChildKeys();
		return _Default ?? new List<string>();
	}

	public static List<int> GetIntList(this DataSnapshot _DataSnapshot, string _Name)
	{
		if (!_DataSnapshot.HasChild(_Name))
			return new List<int>();
		
		return _DataSnapshot.Child(_Name).Children.Select(_Entry => _Entry.GetInt()).ToList();
	}

	public static List<long> GetLongList(this DataSnapshot _DataSnapshot, string _Name)
	{
		if (!_DataSnapshot.HasChild(_Name))
			return new List<long>();
		
		return _DataSnapshot.Child(_Name).Children.Select(_Entry => _Entry.GetLong()).ToList();
	}

	public static Dictionary<string, long> GetLongDictionary(this DataSnapshot _DataSnapshot, string _Name)
	{
		if (!_DataSnapshot.HasChild(_Name))
			return new Dictionary<string, long>();
		
		return _DataSnapshot.Child(_Name).Children.ToDictionary(_Entry => _Entry.Key, _Entry => _Entry.GetLong());
	}

	public static Dictionary<string, string> GetStringDictionary(this DataSnapshot _DataSnapshot, string _Name)
	{
		if (!_DataSnapshot.HasChild(_Name))
			return new Dictionary<string, string>();
		
		return _DataSnapshot.Child(_Name).Children.ToDictionary(_Entry => _Entry.Key, _Entry => _Entry.GetString());
	}
}
