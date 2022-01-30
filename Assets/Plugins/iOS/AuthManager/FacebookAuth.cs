using System;
using System.Threading.Tasks;
using Facebook.Unity;
using Firebase.Auth;
using UnityEngine;

public static class FacebookAuth
{
	static async Task InitializeFacebook()
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
		await InitializeFacebook();
		
		if (AccessToken.CurrentAccessToken != null && AccessToken.CurrentAccessToken.ExpirationTime < DateTime.UtcNow)
			return AccessToken.CurrentAccessToken.TokenString;
		
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		
		FB.LogInWithReadPermissions(
			new string[] { "public_profile", "email" },
			_Result =>
			{
				if (_Result.Cancelled)
					completionSource.SetCanceled();
				else if (!string.IsNullOrEmpty(_Result.Error))
					completionSource.SetException(new Exception($"[FacebookAuthManager] Login failed. Error: {_Result.Error}."));
				else
				{
					Debug.LogFormat("[FacebookAuthManager] Login success. Access Token: {0}.", _Result.AccessToken.TokenString);
					completionSource.SetResult(_Result.AccessToken.TokenString);
				}
			}
		);
		
		return await completionSource.Task;
	}
}