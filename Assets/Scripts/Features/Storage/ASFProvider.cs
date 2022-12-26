using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioBox.ASF;

public class ASFProvider : JsonProvider
{
	public Task<bool> UploadAsync(
		string            _Path,
		ASFFile           _ASF,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		IDictionary<string, object> data = _ASF.Serialize();
		
		return base.UploadAsync(_Path, data, Encoding.UTF8, _Progress, _Token);
	}

	public Task<ASFFile> DownloadAsync(
		string            _Path,
		Action<float>     _Progress = null,
		CancellationToken _Token    = default
	)
	{
		return DownloadAsync(_Path, _Progress, Encoding.UTF8, _Token).ContinueWith(_Task => new ASFFile(_Task.Result), _Token);
	}
}
