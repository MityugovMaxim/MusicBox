using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OggVorbisEncoder;
using UnityEngine;
using Random = System.Random;

public class AudioStream
{
	const int BUFFER_SIZE = 4096;

	int            Samples   => m_AudioClip.samples;
	int            Channels  => m_AudioClip.channels;
	int            Frequency => m_AudioClip.frequency;
	public float[] Data      => m_Buffer;
	public int     Size      => BUFFER_SIZE;
	public float   Progress  => Mathf.Clamp01((float)m_Sample / Samples);

	readonly AudioClip m_AudioClip;
	readonly float[]   m_Buffer;

	int m_Sample;

	public AudioStream(AudioClip _AudioClip)
	{
		m_AudioClip = _AudioClip;
		m_Buffer    = new float[BUFFER_SIZE * Channels];
		m_Sample    = 0;
	}

	public bool Read()
	{
		if (m_Sample >= Samples)
			return false;
		
		m_AudioClip.GetData(m_Buffer, m_Sample);
		
		int offset = m_Buffer.Length / Channels;
		
		m_Sample += offset;
		
		return true;
	}

	public bool Read(ref byte[] _Bytes)
	{
		if (!Read())
			return false;
		
		const int step = sizeof(float);
		const int size = BUFFER_SIZE * step;
		
		if (_Bytes == null)
			_Bytes = new byte[size];
		else if (_Bytes.Length != size)
			Array.Resize(ref _Bytes, size);
		
		Span<byte> value = new Span<byte>(new byte[4]);
		
		for (int i = 0; i < m_Buffer.Length; i++)
		{
			BitConverter.TryWriteBytes(value, m_Buffer.Length);
			for (int j = 0; j < value.Length; j++)
				_Bytes[i * step + j] = value[j];
		}
		
		return true;
	}
}

public static class AudioClipExtension
{
	public static AudioClip Trim(this AudioClip _AudioClip, float _StartTime, float _EndTime)
	{
		const int chunk = 1024;
		
		if (_AudioClip == null)
			return null;
		
		float duration = _EndTime - _StartTime;
		
		if (duration < float.Epsilon)
			return null;
		
		int channels = _AudioClip.channels;
		int samples  = (int)(duration * _AudioClip.frequency);
		int position = (int)(Mathf.Clamp(_StartTime, 0, _AudioClip.length - duration) * _AudioClip.frequency);
		
		AudioClip audioClip = AudioClip.Create(
			_AudioClip.name,
			samples,
			_AudioClip.channels,
			_AudioClip.frequency,
			false
		);
		
		float[] buffer = new float[chunk * channels];
		
		int offset = 0;
		while (offset < samples && _AudioClip.GetData(buffer, position + offset))
		{
			audioClip.SetData(buffer, offset);
			
			offset += chunk;
		}
		
		return audioClip;
	}

	public static AudioClip Clone(this AudioClip _AudioClip)
	{
		if (_AudioClip == null)
			return null;
		
		const int chunk = 1024;
		
		AudioClip audioClip = AudioClip.Create(
			_AudioClip.name,
			_AudioClip.samples,
			_AudioClip.channels,
			_AudioClip.frequency,
			false
		);
		
		int channels = _AudioClip.channels;
		int samples  = _AudioClip.samples;
		
		float[] buffer = new float[chunk * channels];
		
		int offset = 0;
		while (offset <= samples && _AudioClip.GetData(buffer, offset))
		{
			audioClip.SetData(buffer, offset);
			
			offset += chunk;
		}
		
		return audioClip;
	}

	public static void FadeIn(this AudioClip _AudioClip, float _Duration, EaseFunction _Function)
	{
		if (_AudioClip == null || _Duration < float.Epsilon)
			return;
		
		int channels = _AudioClip.channels;
		int samples  = (int)(Mathf.Clamp(_Duration, 0, _AudioClip.length) * _AudioClip.frequency);
		
		float[] buffer = new float[samples * channels];
		
		_AudioClip.GetData(buffer, 0);
		
		for (int i = 0; i < buffer.Length; i++)
		{
			float value = Mathf.InverseLerp(0, buffer.Length - 1, i);
			
			float phase = _Function.Get(value);
			
			buffer[i] *= phase;
		}
		
		_AudioClip.SetData(buffer, 0);
	}

	public static void FadeOut(this AudioClip _AudioClip, float _Duration, EaseFunction _Function)
	{
		if (_AudioClip == null || _Duration < float.Epsilon)
			return;
		
		int channels = _AudioClip.channels;
		int samples  = (int)(Mathf.Clamp(_Duration, 0, _AudioClip.length) * _AudioClip.frequency);
		int offset   = _AudioClip.samples - samples;
		
		float[] buffer = new float[samples * channels];
		
		_AudioClip.GetData(buffer, offset);
		
		for (int i = 0; i < buffer.Length; i++)
		{
			float value = Mathf.InverseLerp(buffer.Length - 1, 0, i);
			
			float phase = _Function.Get(value);
			
			buffer[i] *= phase;
		}
		
		_AudioClip.SetData(buffer, offset);
	}

	public static void FadeInOut(this AudioClip _AudioClip, float _Duration, EaseFunction _Function)
	{
		if (_AudioClip == null || _Duration < float.Epsilon)
			return;
		
		int channels = _AudioClip.channels;
		int samples  = (int)(Mathf.Clamp(_Duration, 0, _AudioClip.length) * _AudioClip.frequency);
		int offset   = _AudioClip.samples - samples;
		
		float[] buffer = new float[samples * channels];
		
		_AudioClip.GetData(buffer, 0);
		
		for (int i = 0; i < buffer.Length; i++)
		{
			float value = Mathf.InverseLerp(0, buffer.Length - 1, i);
			
			float phase = _Function.Get(value);
			
			buffer[i] *= phase;
		}
		
		_AudioClip.SetData(buffer, 0);
		
		_AudioClip.GetData(buffer, offset);
		
		for (int i = 0; i < buffer.Length; i++)
		{
			float value = Mathf.InverseLerp(buffer.Length - 1, 0, i);
			
			float phase = _Function.Get(value);
			
			buffer[i] *= phase;
		}
		
		_AudioClip.SetData(buffer, offset);
	}

	public static async Task<string> CacheAsync(this AudioClip _AudioClip)
	{
		string name = Guid.NewGuid().ToString();
		
		string path = $"{Application.temporaryCachePath}/{name}.cache";
		
		await using FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
		
		const int size = 4096;
		const int step = sizeof(float);
		
		int samples  = _AudioClip.samples;
		int channels = _AudioClip.channels;
		
		int offset = 0;
		
		float[] buffer = new float[size * channels];
		byte[]  data   = new byte[buffer.Length * step];
		
		while (offset < samples)
		{
			_AudioClip.GetData(buffer, offset);
			
			for (int i = 0; i < buffer.Length; i++)
			{
				byte[] value = BitConverter.GetBytes(buffer[i]);
				for (int j = 0; j < value.Length; j++)
					data[i * step + j] = value[j];
			}
			
			await stream.WriteAsync(data);
			
			offset += size;
		}
		
		return path;
	}

	public static async Task<string> CacheOGG(this AudioClip _AudioClip, float _Quality, Dictionary<string, string> _Tags = null, Action<float> _Progress = null)
	{
		string name = Guid.NewGuid().ToString();
		
		string path = $"{Application.temporaryCachePath}/{name}.ogg";
		
		await using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
		
		AudioStream audioStream = new AudioStream(_AudioClip);
		
		VorbisInfo vorbisInfo = VorbisInfo.InitVariableBitRate(
			_AudioClip.channels,
			_AudioClip.frequency,
			_Quality
		);
		
		int serial = new Random().Next();
		
		OggStream stream = new OggStream(serial);
		
		ProcessingState state = ProcessingState.Create(vorbisInfo);
		
		Comments comments = new Comments();
		if (_Tags != null)
		{
			foreach (var entry in _Tags)
				comments.AddTag(entry.Key, entry.Value);
		}
		
		OggPacket[] packets =
		{
			HeaderPacketBuilder.BuildInfoPacket(vorbisInfo),
			HeaderPacketBuilder.BuildCommentsPacket(comments),
			HeaderPacketBuilder.BuildBooksPacket(vorbisInfo),
		};
		
		foreach (OggPacket packet in packets)
			stream.PacketIn(packet);
		
		while (stream.PageOut(out OggPage page, true))
		{
			fileStream.Write(page.Header);
			
			await fileStream.WriteAsync(page.Body);
		}
		
		_Progress?.Invoke(0);
		
		while (audioStream.Read())
		{
			_Progress?.Invoke(audioStream.Progress);
			
			state.WriteData(audioStream.Data, audioStream.Size);
			
			while (!stream.Finished && state.PacketOut(out OggPacket packet))
			{
				stream.PacketIn(packet);
				
				while (!stream.Finished && stream.PageOut(out OggPage page, false))
				{
					fileStream.Write(page.Header);
					
					await fileStream.WriteAsync(page.Body);
				}
			}
		}
		
		return path;
	}

	public static async Task<byte[]> EncodeToOGG(this AudioClip _AudioClip, float _Quality, Dictionary<string, string> _Tags = null)
	{
		const int chunk = 2048;
		
		int samples   = _AudioClip.samples;
		int channels  = _AudioClip.channels;
		int frequency = _AudioClip.frequency;
		
		VorbisInfo vorbisInfo = VorbisInfo.InitVariableBitRate(channels, frequency, _Quality);
		
		Comments comments = new Comments();
		if (_Tags != null)
		{
			foreach (var entry in _Tags)
				comments.AddTag(entry.Key, entry.Value);
		}
		
		OggPacket[] packets =
		{
			HeaderPacketBuilder.BuildInfoPacket(vorbisInfo),
			HeaderPacketBuilder.BuildCommentsPacket(comments),
			HeaderPacketBuilder.BuildBooksPacket(vorbisInfo),
		};
		
		int serial = new Random().Next();
		
		OggStream stream = new OggStream(serial);
		
		foreach (OggPacket packet in packets)
			stream.PacketIn(packet);
		
		List<byte> buffer = new List<byte>(samples * channels);
		
		while (stream.PageOut(out OggPage page, true))
		{
			buffer.AddRange(page.Header);
			buffer.AddRange(page.Body);
		}
		
		ProcessingState state = ProcessingState.Create(vorbisInfo);
		
		int offset = 0;
		
		float[] data = new float[chunk * channels];
		while (offset <= samples && _AudioClip.GetData(data, offset))
		{
			await state.WriteDataAsync(data, chunk);
			
			while (!stream.Finished && state.PacketOut(out OggPacket packet))
			{
				stream.PacketIn(packet);
				
				while (!stream.Finished && stream.PageOut(out OggPage page, false))
				{
					buffer.AddRange(page.Header);
					buffer.AddRange(page.Body);
				}
			}
			
			offset += chunk;
		}
		
		state.WriteEndOfStream();
		
		return buffer.ToArray();
	}
}