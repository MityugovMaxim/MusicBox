using System.Collections.Generic;
using OggVorbisEncoder;
using UnityEngine;
using Random = System.Random;

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
		int position = (int)(Mathf.Clamp(_StartTime, 0, _AudioClip.length) * _AudioClip.frequency);
		
		AudioClip audioClip = AudioClip.Create(
			_AudioClip.name,
			samples,
			_AudioClip.channels,
			_AudioClip.frequency,
			false
		);
		
		float[] buffer = new float[chunk * channels];
		
		int offset = 0;
		while (offset <= samples && _AudioClip.GetData(buffer, position + offset))
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

	public static AudioClip FadeInOut(this AudioClip _AudioClip, float _Duration)
	{
		return _AudioClip.FadeInOut(_Duration, EaseFunction.Linear);
	}

	public static AudioClip FadeInOut(this AudioClip _AudioClip, float _Duration, EaseFunction _Function)
	{
		if (_AudioClip == null || _Duration < float.Epsilon)
			return _AudioClip;
		
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
		
		return _AudioClip;
	}

	public static byte[] EncodeToOGG(this AudioClip _AudioClip, float _Quality, Dictionary<string, string> _Tags = null)
	{
		const int chunk = 1024;
		
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
			state.WriteData(data, chunk);
			
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