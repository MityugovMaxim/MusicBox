using System;
using System.Threading.Tasks;
using Firebase.Auth;

public static class AppleAuth
{
	delegate void AppleAuthSuccessCallback(string _IDToken, string _Nonce, string _DisplayName);

	delegate void AppleAuthCanceledCallback();

	delegate void AppleAuthFailedCallback(string _Error);

	#if UNITY_IOS && !UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("__Internal")]
	static extern void AppleAuthManager_Login(AppleAuthSuccessCallback _Success, AppleAuthCanceledCallback _Canceled, AppleAuthFailedCallback _Failed);
	#endif

	static string m_DisplayName;

	static Action<string, string> m_Success;
	static Action                 m_Canceled;
	static Action<string>         m_Failed;

	public static UserProfile GetProfile()
	{
		UserProfile profile = new UserProfile();
		profile.DisplayName = !m_DisplayName.Contains("(null)") ? m_DisplayName : null;
		profile.PhotoUrl    = null;
		return profile;
	}

	public static Task<(string IDToken, string Nonce)> LoginAsync()
	{
		TaskCompletionSource<(string, string)> completionSource = new TaskCompletionSource<(string, string)>();
		
		m_Success  = (_IDToken, _Nonce) => completionSource.TrySetResult((_IDToken, _Nonce));
		m_Canceled = () => completionSource.TrySetCanceled();
		m_Failed   = _Error => completionSource.TrySetException(new Exception(_Error));
		
		#if UNITY_IOS && !UNITY_EDITOR
		AppleAuthManager_Login(InvokeLoginSuccess, InvokeLoginCanceled, InvokeLoginFailed);
		#else
		InvokeLoginFailed("[AppleAuthManager] Login failed. Apple sign in is not supported.");
		#endif
		
		return completionSource.Task;
	}

	[AOT.MonoPInvokeCallback(typeof(AppleAuthSuccessCallback))]
	static void InvokeLoginSuccess(string _IDToken, string _Nonce, string _DisplayName)
	{
		m_DisplayName = _DisplayName;
		
		Action<string, string> action = m_Success;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke(_IDToken, _Nonce);
	}

	[AOT.MonoPInvokeCallback(typeof(AppleAuthCanceledCallback))]
	static void InvokeLoginCanceled()
	{
		Action action = m_Canceled;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke();
	}

	[AOT.MonoPInvokeCallback(typeof(AppleAuthFailedCallback))]
	static void InvokeLoginFailed(string _Error)
	{
		Action<string> action = m_Failed;
		m_Success  = null;
		m_Canceled = null;
		m_Failed   = null;
		action?.Invoke(_Error);
	}
}
