#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using UnityEngine.Scripting;

[Preserve]
public class iOSFileManager : IFileManager
{
	public delegate void SelectSuccess(string _URL);

	public delegate void SelectFailed(string _Error);

	public delegate void SelectCanceled();

	[DllImport("__Internal")]
	static extern void SelectFile(string _Extension, SelectSuccess _Success, SelectFailed _Failed, SelectCanceled _Canceled);

	static Action<string> m_SelectSuccess;
	static Action         m_SelectCanceled;
	static Action<string> m_SelectFailed;

	public Task<string> SelectFile(string _Extension, CancellationToken _Token = default)
	{
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		_Token.Register(() => completionSource.SetCanceled());
		
		m_SelectSuccess  = _URL => completionSource.SetResult(_URL);
		m_SelectFailed   = _Error => completionSource.SetException(new Exception(_Error));
		m_SelectCanceled = () => completionSource.SetCanceled();
		
		SelectFile(_Extension, InvokeSelectSuccess, InvokeSelectFailed, InvokeSelectCanceled);
		
		return completionSource.Task;
	}

	[MonoPInvokeCallback(typeof(SelectSuccess))]
	static void InvokeSelectSuccess(string _URL)
	{
		Action<string> action = m_SelectSuccess;
		m_SelectSuccess  = null;
		m_SelectCanceled = null;
		m_SelectFailed   = null;
		action?.Invoke(_URL);
	}

	[MonoPInvokeCallback(typeof(SelectCanceled))]
	static void InvokeSelectCanceled()
	{
		Action action = m_SelectCanceled;
		m_SelectSuccess  = null;
		m_SelectCanceled = null;
		m_SelectFailed   = null;
		action?.Invoke();
	}

	[MonoPInvokeCallback(typeof(SelectFailed))]
	static void InvokeSelectFailed(string _Error)
	{
		Action<string> action = m_SelectFailed;
		m_SelectSuccess  = null;
		m_SelectCanceled = null;
		m_SelectFailed   = null;
		action?.Invoke(_Error);
	}
}
#endif