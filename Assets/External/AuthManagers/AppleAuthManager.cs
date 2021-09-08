using System;

public static class AppleAuthManager
{
	delegate void AppleAuthCallback(string _Data);

	#if UNITY_IOS && !UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("__Internal")]
	static extern void AppleAuthManager_Login(string _Nonce, AppleAuthCallback _Success, AppleAuthCallback _Failed);
	#endif

	static Action<string> m_LoginSuccess;
	static Action<string> m_LoginFailed;

	public static void Login(
		string         _Nonce,
		Action<string> _Success,
		Action<string> _Failed
	)
	{
		m_LoginSuccess = _Success;
		m_LoginFailed  = _Failed;
		
		#if UNITY_IOS && !UNITY_EDITOR
		AppleAuthManager_Login(_Nonce, LoginSuccess, LoginFailed);
		#else
		LoginFailed("Apple auth not supported by editor.");
		#endif
	}

	[AOT.MonoPInvokeCallback(typeof(AppleAuthCallback))]
	static void LoginSuccess(string _Token)
	{
		Action<string> action = m_LoginSuccess;
		m_LoginSuccess = null;
		action?.Invoke(_Token);
	}

	[AOT.MonoPInvokeCallback(typeof(AppleAuthCallback))]
	static void LoginFailed(string _Error)
	{
		Action<string> action = m_LoginFailed;
		m_LoginFailed = null;
		action?.Invoke(_Error);
	}
}
