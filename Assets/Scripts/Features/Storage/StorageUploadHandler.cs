using System;
using System.Threading.Tasks;
using Firebase.Storage;

public class StorageUploadHandler : IProgress<UploadState>, IDisposable
{
	public Task<bool> Task { get; private set; }

	Action<float> m_Progress;

	float m_Value;

	public StorageUploadHandler(Task<bool> _Task, Action<float> _Progress)
	{
		Task       = _Task;
		m_Progress = _Progress;
		m_Value    = 0;
	}

	public void AddListener(Action<float> _Progress)
	{
		if (_Progress == null)
			return;
		
		_Progress.Invoke(m_Value);
		
		m_Progress += _Progress;
	}

	public void RemoveListener(Action<float> _Progress)
	{
		if (_Progress == null)
			return;
		
		_Progress.Invoke(m_Value);
		
		m_Progress -= _Progress;
	}

	public void Report(UploadState _State)
	{
		if (m_Progress == null)
			return;
		
		double source = _State.BytesTransferred;
		double target = _State.TotalByteCount;
		
		m_Value = MathUtility.Remap01(source, 0, target);
		
		m_Progress.Invoke(m_Value);
	}

	public void Dispose()
	{
		Task       = null;
		m_Progress = null;
	}
}