#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class iOSGoogleAuth
{
	delegate void GoogleAuthSuccessCallback(string _IDToken, string _AccessToken);

	delegate void GoogleAuthCanceledCallback();

	delegate void GoogleAuthFailedCallback(string _Error);

	[DllImport("__Internal")]
	static extern void GoogleAuthManager_Login(GoogleAuthSuccessCallback _Success, GoogleAuthCanceledCallback _Canceled, GoogleAuthFailedCallback _Failed);

	static Action<string, string> m_Success;
	static Action                 m_Canceled;
	static Action<string>         m_Failed;

	public static Task<(string IDToken, string AccessToken)> LoginAsync()
	{
		TaskCompletionSource<(string, string)> completionSource = new TaskCompletionSource<(string, string)>();
		
		m_Success  = (_IDToken, _AccessToken) => completionSource.TrySetResult((_IDToken, _AccessToken));
		m_Canceled = () => completionSource.TrySetCanceled();
		m_Failed   = _Error => completionSource.TrySetException(new Exception(_Error));
		
		GoogleAuthManager_Login(InvokeLoginSuccess, InvokeLoginCanceled, InvokeLoginFailed);
		
		return completionSource.Task;
	}

	[AOT.MonoPInvokeCallback(typeof(GoogleAuthSuccessCallback))]
	static void InvokeLoginSuccess(string _IDToken, string _AccessToken)
	{
		Action<string, string> action = m_Success;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke(_IDToken, _AccessToken);
	}

	[AOT.MonoPInvokeCallback(typeof(GoogleAuthCanceledCallback))]
	static void InvokeLoginCanceled()
	{
		Action action = m_Canceled;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke();
	}

	[AOT.MonoPInvokeCallback(typeof(GoogleAuthFailedCallback))]
	static void InvokeLoginFailed(string _Error)
	{
		Action<string> action = m_Failed;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke(_Error);
	}
}
#endif