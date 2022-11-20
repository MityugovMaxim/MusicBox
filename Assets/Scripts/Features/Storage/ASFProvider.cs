using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ASFProvider : JsonProvider
{
	public Task<bool> UploadAsync(
		string                     _Path,
		Dictionary<string, object> _Json,
		Action<float>              _Progress = null,
		CancellationToken          _Token    = default
	)
	{
		return base.UploadAsync(_Path, _Json, Encoding.UTF8, _Progress, _Token);
	}

	public Task<Dictionary<string, object>> DownloadAsync(
		string            _Path,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		return DownloadAsync(_Path, _Progress, Encoding.UTF8, _Token);
	}
}
