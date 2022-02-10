using System;
using System.Threading.Tasks;
using Facebook.Unity;
using Firebase.Auth;

public static class FacebookAuth
{
	public static async Task Initialize()
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (FB.IsInitialized)
			completionSource.SetResult(true);
		else
			FB.Init(() => completionSource.SetResult(true));
		
		await completionSource.Task;
		
		FB.ActivateApp();
	}

	public static UserProfile GetProfile()
	{
		UserProfile profile = new UserProfile();
		
		Profile data = FB.Mobile.CurrentProfile();
		if (data != null)
		{
			profile.DisplayName = data.Name;
			profile.PhotoUrl    = new Uri(data.ImageURL);
		}
		
		return profile;
	}

	public static async Task<string> LoginAsync()
	{
		await Initialize();
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		FB.LogInWithReadPermissions(
			new string[] { "public_profile", "email" },
			_Result =>
			{
				if (_Result.Cancelled)
					completionSource.TrySetCanceled();
				else if (!string.IsNullOrEmpty(_Result.Error))
					completionSource.TrySetException(new Exception($"[FacebookAuthManager] Login failed. Error: {_Result.Error}."));
				else
					completionSource.TrySetResult(_Result.AccessToken.TokenString);
			}
		);
		
		return await completionSource.Task;
	}
}