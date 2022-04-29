#if UNITY_ANDROID
using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Scripting;

public static class AndroidGoogleAuth
{
	class GoogleAuthSuccess : AndroidJavaProxy
	{
		readonly Action<string, string> m_Action;

		public GoogleAuthSuccess(Action<string, string> _Action) : base(SUCCESS_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		public void Invoke(string _IDToken, string _AccessToken)
		{
			m_Action?.Invoke(_IDToken, _AccessToken);
		}
	}

	class GoogleAuthCanceled : AndroidJavaProxy
	{
		readonly Action m_Action;

		public GoogleAuthCanceled(Action _Action) : base(CANCEL_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		public void Invoke()
		{
			m_Action?.Invoke();
		}
	}

	class GoogleAuthFailed : AndroidJavaProxy
	{
		readonly Action<string> m_Action;

		public GoogleAuthFailed(Action<string> _Action) : base(FAIL_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		public void Invoke(string _Message)
		{
			m_Action?.Invoke(_Message);
		}
	}

	const string CLASS_NAME   = "com.audiobox.auth.AuthManager";
	const string SUCCESS_NAME = "com.audiobox.auth.AuthSuccessHandler";
	const string CANCEL_NAME  = "com.audiobox.auth.AuthCancelHandler";
	const string FAIL_NAME    = "com.audiobox.auth.AuthFailHandler";

	static Action<string, string> m_Success;
	static Action                 m_Canceled;
	static Action<string>         m_Failed;

	public static UserProfile GetProfile()
	{
		return new UserProfile()
		{
			DisplayName = GetName(),
			PhotoUrl    = new Uri(GetPhoto())
		};
	}

	static string GetEmail()
	{
		using (AndroidJavaClass auth = new AndroidJavaClass(CLASS_NAME))
		{
			return auth.CallStatic<string>("GetEmail");
		}
	}

	static string GetName()
	{
		using (AndroidJavaClass auth = new AndroidJavaClass(CLASS_NAME))
		{
			return auth.CallStatic<string>("GetName");
		}
	}

	static string GetPhoto()
	{
		using (AndroidJavaClass auth = new AndroidJavaClass(CLASS_NAME))
		{
			return auth.CallStatic<string>("GetPhoto");
		}
	}

	public static Task<(string IDToken, string AccessToken)> LoginAsync()
	{
		TaskCompletionSource<(string, string)> completionSource = new TaskCompletionSource<(string, string)>();
		
		m_Success  = (_IDToken, _AccessToken) => completionSource.TrySetResult((_IDToken, _AccessToken));
		m_Canceled = () => completionSource.TrySetCanceled();
		m_Failed   = _Error => completionSource.TrySetException(new Exception(_Error));
		
		using (AndroidJavaClass auth = new AndroidJavaClass(CLASS_NAME))
		{
			AndroidJavaProxy success  = new GoogleAuthSuccess(m_Success);
			AndroidJavaProxy canceled = new GoogleAuthCanceled(m_Canceled);
			AndroidJavaProxy failed   = new GoogleAuthFailed(m_Failed);
			auth.CallStatic("Auth", success, canceled, failed);
		}
		
		return completionSource.Task;
	}
}
#endif