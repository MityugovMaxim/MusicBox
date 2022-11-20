using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class LocalizationProvider : JsonProvider
{
	public Task<bool> UploadAsync(
		string                     _Path,
		Dictionary<string, object> _Json,
		Action<float>              _Progress = null,
		CancellationToken          _Token    = default
	)
	{
		return base.UploadAsync(_Path, _Json, Encoding.Unicode, _Progress, _Token);
	}

	public Task<Dictionary<string, string>> DownloadAsync(
		string            _Path,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		return DownloadAsync(_Path, _Progress, Encoding.Unicode, _Token)
			.ContinueWith(
				_Task =>
				{
					Dictionary<string, string> localization = new Dictionary<string, string>();
					if (_Task.IsCompletedSuccessfully && _Task.Result != null)
					{
						foreach (var entry in _Task.Result)
							localization[entry.Key] = (string)entry.Value;
					}
					return localization;
				},
				CancellationToken.None
			);
	}
}
