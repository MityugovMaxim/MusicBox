using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class AudioSourceExtension
{
	public static Task SetVolumeAsync(this AudioSource _AudioSource, float _Volume, float _Duration, CancellationToken _Token)
	{
		if (_AudioSource == null)
			return Task.CompletedTask;
		
		if (_Token.IsCancellationRequested)
			return Task.FromCanceled(_Token);
		
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
		_Token.ThrowIfCancellationRequested();
		
		if (_AudioSource == null || _AudioSource.clip == null)
			return Task.CompletedTask;
		
		_Token.Register(_AudioSource.Stop);
		
		_AudioSource.time = _Time;
		
		_AudioSource.Play();
		
		return UnityTask.While(() => _AudioSource.isPlaying, _Token);
	}
}