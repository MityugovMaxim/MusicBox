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
}