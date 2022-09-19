#if UNITY_IOS
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
	static extern void SelectFile(string[] _Extensions, int _Count, SelectSuccess _Success, SelectFailed _Failed, SelectCanceled _Canceled);

	static Action<string> m_SelectSuccess;
	static Action         m_SelectCanceled;
	static Action<string> m_SelectFailed;

	public Task<string> SelectFile(string[] _Extensions, CancellationToken _Token = default)
	{
		_Token.ThrowIfCancellationRequested();
		
		if (_Extensions == null || _Extensions.Length == 0)
			return Task.FromCanceled<string>(CancellationToken.None);
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		m_SelectSuccess  = _URL => completionSource.SetResult(_URL);
		m_SelectFailed   = _Error => completionSource.SetException(new Exception(_Error));
		m_SelectCanceled = () => completionSource.SetCanceled();
		
		SelectFile(_Extensions, _Extensions.Length, InvokeSelectSuccess, InvokeSelectFailed, InvokeSelectCanceled);
		
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