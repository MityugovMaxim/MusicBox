using System;
using System.Threading.Tasks;
using Google;

public static class GoogleAuthManager
{
	public static Task<string> LoginAsync(string _ClientID)
	{
		TaskCompletionSource<string> taskSource = new TaskCompletionSource<string>();
		
		Login(
			_ClientID,
			_Token => taskSource.TrySetResult(_Token),
			_Error => taskSource.TrySetException(new Exception(_Error))
		);
		
		return taskSource.Task;
	}

	public static async void Login(
		string         _ClientID,
		Action<string> _Success,
		Action<string> _Failed
	)
	{
		#if UNITY_EDITOR
		_Failed?.Invoke("Google auth not supported by editor.");
		return;
		#endif
		
		GoogleSignIn.Configuration = new GoogleSignInConfiguration
		{
			RequestIdToken = true,
			WebClientId    = _ClientID
		};
		
		Task<GoogleSignInUser> task = GoogleSignIn.DefaultInstance.SignIn();
		
		GoogleSignInUser user = await task;
		
		if (task.IsCanceled)
			_Failed?.Invoke("Google auth canceled.");
		else if (task.IsFaulted)
			_Failed?.Invoke(task.Exception?.Message);
		else
			_Success?.Invoke(task.Result.IdToken);
	}
}