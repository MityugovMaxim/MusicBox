using System.Threading.Tasks;
using Firebase.Auth;

public static class GoogleAuth
{
	public static UserProfile GetProfile()
	{
		#if UNITY_EDITOR
		return null;
		#elif UNITY_IOS
		return null;
		#elif UNITY_ANDROID
		return AndroidGoogleAuth.GetProfile();
		#else
		return null;
		#endif
	}

	public static Task<(string IDToken, string AccessToken)> LoginAsync()
	{
		#if UNITY_EDITOR
		return Task.FromResult<(string, string)>((null, null));
		#elif UNITY_IOS
		return iOSGoogleAuth.LoginAsync();
		#elif UNITY_ANDROID
		return AndroidGoogleAuth.LoginAsync();
		#else
		return Task.FromResult<(string, string)>((null, null));
		#endif
	}
}