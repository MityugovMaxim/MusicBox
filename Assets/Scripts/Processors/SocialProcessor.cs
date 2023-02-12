using System;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class SocialProcessor : IInitializable, IDisposable
{
	public bool   Guest  => m_User?.IsAnonymous ?? true;
	public string UserID => m_User?.UserId;
	public string Email  => m_User?.Email;
	public string Name   => m_User?.DisplayName;
	public Uri    Photo  => m_User?.PhotoUrl;

	public event Action OnLogin;
	public event Action OnLogout;
	public event Action OnNameChange;
	public event Action OnPhotoChange;

	FirebaseAuth m_Auth;
	FirebaseUser m_User;
	bool         m_Online;

	public async Task<bool> Login()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
			return false;
		
		m_Auth = FirebaseAuth.DefaultInstance;
		
		try
		{
			m_User = m_Auth.CurrentUser;
			
			if (m_User != null)
				await m_User.ReloadAsync();
			else
				await AuthAnonymously();
			
			OnLogin?.Invoke();
			
			return true;
		}
		catch (Exception exception)
		{
			Log.Exception(this, exception);
			
			return false;
		}
	}

	public void Logout()
	{
		m_User = null;
		
		m_Auth.SignOut();
		
		OnLogout?.Invoke();
	}

	public async Task SetUsername(string _Username)
	{
		if (m_User == null)
			return;
		
		UserProfile profile = new UserProfile();
		profile.DisplayName = _Username;
		profile.PhotoUrl    = m_User.PhotoUrl;
		
		await m_User.UpdateUserProfileAsync(profile);
		
		OnNameChange?.Invoke();
	}

	public async Task<bool> AttachEmail(string _Email, string _Password)
	{
		try
		{
			Credential credential = EmailAuthProvider.GetCredential(_Email, _Password);
			
			m_User = null;//await Link(credential, _Credential => credential = _Credential);
			
			if (m_User == null)
				m_User = await Auth(credential);
			
			if (m_User == null)
				m_User = await m_Auth.SignInAnonymouslyAsync();
			
			credential.Dispose();
			
			OnLogin?.Invoke();
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Login with email failed. Error: {0}", exception.Message);
		}
		
		return false;
	}

	public async Task<bool> AttachAppleID()
	{
		try
		{
			(string idToken, string nonce) = await AppleAuth.LoginAsync();
			
			UserProfile profile = AppleAuth.GetProfile();
			
			Credential credential = OAuthProvider.GetCredential("apple.com", idToken, nonce, null);
			
			m_User = await Link(credential, _Credential => credential = _Credential);
			
			if (m_User == null)
				m_User = await Auth(credential);
			
			if (m_User == null)
				m_User = await m_Auth.SignInAnonymouslyAsync();
			
			await Merge(profile);
			
			credential.Dispose();
			
			OnLogin?.Invoke();
			
			return true;
		}
		catch (OperationCanceledException)
		{
			Debug.Log("[SocialProcessor] Login with Apple ID canceled.");
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Login with Apple ID failed. Error: {0}", exception.Message);
		}
		
		return false;
	}

	public async Task<bool> AttachGoogleID()
	{
		try
		{
			(string idToken, string accessToken) = await GoogleAuth.LoginAsync();
			
			UserProfile profile = GoogleAuth.GetProfile();
			
			Credential credential = GoogleAuthProvider.GetCredential(idToken, accessToken);
			
			m_User = await Link(credential, _Credential => credential = _Credential);
			
			if (m_User == null)
				m_User = await Auth(credential);
			
			if (m_User == null)
				m_User = await m_Auth.SignInAnonymouslyAsync();
			
			await Merge(profile);
			
			credential.Dispose();
			
			return true;
		}
		catch (OperationCanceledException)
		{
			Debug.Log("[SocialProcessor] Login with Google ID canceled.");
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Login with Google ID failed. Error: {0}", exception.Message);
		}
		
		return false;
	}

	public async Task<bool> AttachFacebookID()
	{
		try
		{
			string accessToken = await FacebookAuth.LoginAsync();
			
			UserProfile profile = FacebookAuth.GetProfile();
			
			Credential credential = FacebookAuthProvider.GetCredential(accessToken);
			
			m_User = await Link(credential, _Credential => credential = _Credential);
			
			if (m_User == null)
				m_User = await Auth(credential);
			
			if (m_User == null)
				m_User = await m_Auth.SignInAnonymouslyAsync();
			
			await Merge(profile);
			
			credential.Dispose();
			
			OnLogin?.Invoke();
			
			return true;
		}
		catch (OperationCanceledException)
		{
			Debug.Log("[SocialProcessor] Login with Facebook ID canceled.");
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Login with Facebook ID failed. Error: {0}", exception.Message);
		}
		
		return false;
	}

	void IInitializable.Initialize()
	{
		m_Auth = FirebaseAuth.DefaultInstance;
		m_User = m_Auth.CurrentUser;
	}

	void IDisposable.Dispose() { }

	async Task AuthAnonymously()
	{
		m_User = await m_Auth.SignInAnonymouslyAsync();
		
		if (m_User == null)
			Debug.LogError("[SocialProcessor] Login anonymously failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login anonymously success. Username: {0}. User ID: {1}", (await m_Auth.SignInAnonymouslyAsync()).DisplayName, (await m_Auth.SignInAnonymouslyAsync()).UserId);
	}

	async Task Merge(UserProfile _Profile)
	{
		if (m_User == null || _Profile == null)
			return;
		
		if (string.IsNullOrEmpty(_Profile.DisplayName))
			_Profile.DisplayName = m_User.DisplayName;
		
		if (string.IsNullOrEmpty(_Profile.PhotoUrl?.ToString()))
			_Profile.PhotoUrl = m_User.PhotoUrl;
		
		try
		{
			await m_User.TokenAsync(true);
			
			await m_User.UpdateUserProfileAsync(_Profile);
		}
		catch (Exception exception)
		{
			Debug.LogWarningFormat("[SocialProcessor] Merge failed. Error: {0}.", exception.Message);
		}
	}

	Task<FirebaseUser> Link(Credential _Credential, Action<Credential> _UpdateCredential = null)
	{
		TaskCompletionSource<FirebaseUser> completionSource = new TaskCompletionSource<FirebaseUser>();
		
		if (m_User == null || !m_User.IsAnonymous)
		{
			completionSource.SetResult(null);
			return completionSource.Task;
		}
		
		m_User.LinkAndRetrieveDataWithCredentialAsync(_Credential).ContinueWith(
			_Task =>
			{
				if (_Task.Exception != null)
				{
					foreach (Exception innerException in _Task.Exception.Flatten().InnerExceptions)
					{
						if (innerException is FirebaseAccountLinkException linkException && linkException.UserInfo.UpdatedCredential.IsValid())
						{
							Debug.LogWarning("[SocialProcessor] Link failed. Received credential.");
							_UpdateCredential?.Invoke(linkException.UserInfo.UpdatedCredential);
							break;
						}
					}
					
					completionSource.SetResult(null);
				}
				else if (_Task.IsCompleted)
				{
					Log.Info(this, "Link success. UserID: {0} Name: {1} Provider: {2}.", m_User.UserId, m_User.DisplayName, _Credential.Provider);
					completionSource.SetResult(m_User);
				}
				else
				{
					Debug.LogFormat("[SocialProcessor] Link failed. UserID: {0} Name: {1} Provider: {2}.", m_User.UserId, m_User.DisplayName, _Credential.Provider);
					completionSource.SetResult(null);
				}
			}
		);
		
		return completionSource.Task;
	}

	async Task<FirebaseUser> Auth(Credential _Credential)
	{
		try
		{
			FirebaseUser user = await m_Auth.SignInWithCredentialAsync(_Credential);
			
			Debug.LogFormat("[SocialProcessor] Auth success. UserID: {0} Name: {1} Provider: {2}.", user.UserId, user.DisplayName,_Credential.Provider);
			
			return user;
		}
		catch (FirebaseException exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Auth failed. Error: {0}.", exception.Message);
			Debug.LogException(exception);
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialProcessor] Auth failed. Error: {0}.", exception.Message);
			Debug.LogException(exception);
		}
		
		return null;
	}
}
