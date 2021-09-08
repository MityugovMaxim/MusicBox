using System;
using System.Threading.Tasks;
using Google;

public static class GoogleAuthManager
{
	public static void Login(
		string         _ClientID,
		Action<string> _Success,
		Action<string> _Failed
	)
	{
		GoogleSignIn.Configuration = new GoogleSignInConfiguration
		{
			RequestIdToken = true,
			WebClientId    = _ClientID
		};
		
		Task<GoogleSignInUser> auth = GoogleSignIn.DefaultInstance.SignIn ();
		
		auth.ContinueWith(
			_Task =>
			{
				if (_Task.IsCanceled)
					_Failed?.Invoke("Google auth canceled.");
				else if (_Task.IsFaulted)
					_Failed?.Invoke(_Task.Exception?.Message);
				else
					_Success?.Invoke(_Task.Result.IdToken);
			}
		);
	}
}