using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SecurePrefs
{
	const string HASH_POSTFIX   = "_HASH";
	const string SECRET_POSTFIX = "_SECRET";

	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	static readonly string[] m_Secret =
	{
		"9Swtr24jaf",
		"7shFhj*ef3",
		"Vy;*Sjflq)",
		"mvkPS94M#*",
		"l7AHvm0J*@",
	};

	public static void Save(string _Key, string _Data)
	{
		int    secret = Random.Range(0, m_Secret.Length);
		string vector = m_Secret[secret];
		string hash   = GetHash($"{secret}_{vector}_{_Data}");
		
		PlayerPrefs.SetString(_Key, _Data);
		PlayerPrefs.SetInt(_Key + SECRET_POSTFIX, secret);
		PlayerPrefs.SetString(_Key + HASH_POSTFIX, hash);
	}

	public static string Load(string _Key, string _Default = null)
	{
		if (!PlayerPrefs.HasKey(_Key))
			return _Default;
		
		string data = PlayerPrefs.GetString(_Key, string.Empty);
		
		int secret = PlayerPrefs.GetInt(_Key + SECRET_POSTFIX, -1);
		
		if (secret < 0 || secret >= m_Secret.Length)
		{
			Debug.LogError("[SecurePrefs] Load failed. Invalid secret key.");
			PlayerPrefs.DeleteKey(_Key);
			PlayerPrefs.DeleteKey(_Key + SECRET_POSTFIX);
			PlayerPrefs.DeleteKey(_Key + HASH_POSTFIX);
			return _Default;
		}
		
		string hash = PlayerPrefs.GetString(_Key + HASH_POSTFIX, string.Empty);
		
		if (string.IsNullOrEmpty(hash))
		{
			Debug.LogError("[SecurePrefs] Load failed. Invalid hash.");
			PlayerPrefs.DeleteKey(_Key);
			PlayerPrefs.DeleteKey(_Key + SECRET_POSTFIX);
			PlayerPrefs.DeleteKey(_Key + HASH_POSTFIX);
			return _Default;
		}
		
		string key = m_Secret[secret];
		
		if (hash != GetHash($"{secret}_{key}_{data}"))
		{
			Debug.LogError("[SecurePrefs] Load failed. Hash mismatch.");
			PlayerPrefs.DeleteKey(_Key);
			PlayerPrefs.DeleteKey(_Key + SECRET_POSTFIX);
			PlayerPrefs.DeleteKey(_Key + HASH_POSTFIX);
			return _Default;
		}
		
		return data;
	}

	static string GetHash(string _Value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(_Value);
		
		MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
		
		byte[] hashBytes = hashProvider.ComputeHash(bytes);
		
		StringBuilder hash = new StringBuilder();
		foreach (byte hashByte in hashBytes)
			hash.Append(Convert.ToString(hashByte, 16).PadLeft(2, '0'));
		return hash.ToString().PadLeft(32, '0');
	}
}