using System;
using Firebase.Database;

public static class DataSnapshotExtension
{
	public static string GetString(this DataSnapshot _DataSnapshot)
	{
		return (string)_DataSnapshot.Value;
	}

	public static int GetInt(this DataSnapshot _DataSnapshot)
	{
		return int.Parse(_DataSnapshot.GetRawJsonValue());
	}

	public static long GetLong(this DataSnapshot _DataSnapshot)
	{
		return (long)_DataSnapshot.Value;
	}

	public static T GetEnum<T>(this DataSnapshot _DataSnapshot) where T : Enum
	{
		return (T)Enum.Parse(typeof(T), _DataSnapshot.GetRawJsonValue());
	}
}