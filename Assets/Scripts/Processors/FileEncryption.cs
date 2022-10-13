using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

public static class FileEncryption
{
	static Type Tag => typeof(FileEncryption);

	static readonly byte[] m_Key =
	{
		0x11,
		0x17,
		0x1D,
		0x1F,
	};

	public static async Task Save(string _Path, byte[] _Data, CancellationToken _Token = default)
	{
		if (string.IsNullOrEmpty(_Path))
		{
			Log.Error(Tag, "Save file failed. Path is null or empty.");
			return;
		}
		
		if (_Data == null || _Data.Length == 0)
		{
			Log.Error(Tag, "Save file failed. Data is null or empty.");
			return;
		}
		
		Process(_Data);
		
		await using FileStream stream = new FileStream(_Path, FileMode.OpenOrCreate);
		
		await stream.WriteAsync(_Data, 0, _Data.Length, _Token);
		
		await stream.FlushAsync(_Token);
		
		stream.Close();
	}

	public static async Task<T> Load<T>(
		string                                   _Path,
		Func<string, CancellationToken, Task<T>> _Request,
		CancellationToken                        _Token
	) where T : Object
	{
		return await Unpack(_Path, _Request, _Token);
	}

	static async Task<T> Unpack<T>(
		string                                   _Path,
		Func<string, CancellationToken, Task<T>> _Request,
		CancellationToken                        _Token = default
	) where T : Object
	{
		if (string.IsNullOrEmpty(_Path))
		{
			Log.Error(Tag, "Unpack file failed. Path is null or empty.");
			return null;
		}
		
		if (!File.Exists(_Path))
		{
			Log.Error(Tag, "Unpack file failed. File not found at path '{0}'.", _Path);
			return null;
		}
		
		string cache = Path.Combine(
			Application.temporaryCachePath,
			$"{CRC32.Get(_Path)}.ogg"
		);
		
		await using FileStream source = new FileStream(_Path, FileMode.Open, FileAccess.Read);
		await using FileStream target = new FileStream(cache, FileMode.OpenOrCreate, FileAccess.Write);
		
		byte[] buffer = new byte[32768];
		
		int count;
		
		while ((count = await source.ReadAsync(buffer, 0, buffer.Length, _Token)) > 0)
		{
			Process(buffer);
			
			await target.WriteAsync(buffer, 0, count, _Token);
		}
		
		await source.FlushAsync(_Token);
		await target.FlushAsync(_Token);
		
		source.Close();
		target.Close();
		
		return await _Request.Invoke(cache, _Token);
	}

	static void Process(byte[] _Data)
	{
		int length = m_Key.Length;
		for (int i = 0; i < _Data.Length; i++)
			_Data[i] ^= m_Key[i % length];
	}
}
