using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class AudioSourceExtension
{
	public static Task SetVolumeAsync(this AudioSource _AudioSource, float _Volume, float _Duration, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (_AudioSource == null || _AudioSource.clip == null)
			return Task.CompletedTask;
		
		float source = _AudioSource.volume;
		float target = Mathf.Clamp01(_Volume);
		
		return UnityTask.Lerp(
			_Value => _AudioSource.volume = _Value,
			source,
			target,
			_Duration,
			_Token
		);
	}

	public static Task PlayAsync(this AudioSource _AudioSource, CancellationToken _Token = default)
	{
		return _AudioSource.PlayAsync(0, _Token);
	}

	public static Task PlayAsync(this AudioSource _AudioSource, float _Time, CancellationToken _Token = default)
	{
		return _AudioSource.PlayAsync(_Time, float.MaxValue, _Token);
	}

	public static Task PlayAsync(this AudioSource _AudioSource, float _Time, float _Length, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (_AudioSource == null || _AudioSource.clip == null)
			return Task.CompletedTask;
		
		float source = _Time;
		float target = _Time + _Length;
		
		source = Mathf.Clamp(source, 0, _AudioSource.clip.length);
		target = Mathf.Clamp(target, 0, _AudioSource.clip.length);
		
		_AudioSource.time = source;
		
		_AudioSource.Play();
		
		return UnityTask.While(() => _AudioSource.isPlaying && _AudioSource.time <= target, _Token);
	}
}
