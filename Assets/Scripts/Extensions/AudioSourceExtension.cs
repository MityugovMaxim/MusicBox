using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class AudioSourceExtension
{
	public static async Task SetVolumeAsync(this AudioSource _AudioSource, float _Volume, float _Duration, CancellationToken _Token)
	{
		if (_AudioSource == null || _Token.IsCancellationRequested)
			return;
		
		float source = _AudioSource.volume;
		float target = Mathf.Clamp01(_Volume);
		
		if (!Mathf.Approximately(source, target) && _Duration > float.Epsilon)
		{
			float time = 0;
			while (time < _Duration)
			{
				await UnityTask.Yield(CancellationToken.None);
				
				if (_Token.IsCancellationRequested)
					return;
				
				time += Time.deltaTime;
				
				_AudioSource.volume = Mathf.Lerp(source, target, time / _Duration);
			}
		}
		
		_AudioSource.volume = target;
	}
}