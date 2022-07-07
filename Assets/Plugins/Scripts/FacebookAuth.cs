using System;
using System.Threading.Tasks;
using Facebook.Unity;
using Firebase.Auth;

public static class FacebookAuth
{
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
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		FB.LogInWithReadPermissions(
			new string[] { "public_profile", "email", "user_friends" },
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