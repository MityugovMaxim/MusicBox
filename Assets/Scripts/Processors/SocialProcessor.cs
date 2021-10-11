using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

[Preserve]
public class LoginSignal { }

[Preserve]
public class LogoutSignal { }

[Preserve]
public class SocialProcessor : IInitializable, IDisposable
{
	public bool Online
	{
		get => m_Online && Application.internetReachability != NetworkReachability.NotReachable;
		private set => m_Online = value;
	}

	public bool   Guest  => m_User?.IsAnonymous ?? true;
	public string UserID => m_User?.UserId;
	public string Email  => m_User?.Email;
	public string Name   => m_User?.DisplayName;
	public Uri    Photo  => m_User?.PhotoUrl;

	readonly SignalBus m_SignalBus;

	FirebaseAuth m_Auth;
	FirebaseUser m_User;
	bool         m_Online;

	[Inject]
	public SocialProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public async Task Login()
	{
		try
		{
			if (m_User == null)
				await m_Auth.SignInAnonymouslyAsync();
			
			if (m_User != null)
				await m_User.ReloadAsync();
			
			Online = true;
		}
		catch (Exception)
		{
			Debug.LogWarning("[SocialProcessor] Login failed. Entering offline mode.");
			
			Online = false;
		}
	}

	public void Logout()
	{
		m_Auth.SignOut();
	}

	public async Task<bool> AttachAppleID()
	{
		try
		{
			string nonce = Guid.NewGuid().ToString();
			
			string token = await AppleAuthManager.LoginAsync(nonce);
			
			await AppleAuth(token, nonce);
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialManager] Login with Apple ID failed. Error: {0}", exception.Message);
			
			return false;
		}
	}

	public async Task<bool> AttachGoogleID()
	{
		try
		{
			string clientID = "266200973318-r1sbm8gud1amf7rvd04u8shn2mqkt3ci.apps.googleusercontent.com";
			
			string token = await GoogleAuthManager.LoginAsync(clientID);
			
			await GoogleAuth(clientID, token);
			
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("[SocialManager] Login with Google ID failed. Error: {0}", exception.Message);
			
			return false;
		}
	}

	void IInitializable.Initialize()
	{
		m_Auth = FirebaseAuth.DefaultInstance;
		
		m_Auth.StateChanged += StateChanged;
		
		StateChanged(this, null);
	}

	void IDisposable.Dispose()
	{
		m_Auth.StateChanged -= StateChanged;
	}

	void StateChanged(object _Sender, EventArgs _Args)
	{
		FirebaseUser user = m_Auth.CurrentUser;
		
		if (m_User == user)
			return;
		
		if (user != null)
		{
			Debug.LogFormat("[SocialProcessor] User login. User ID: {0}.", user.UserId);
			m_User = user;
			m_SignalBus.Fire<LoginSignal>();
		}
		else
		{
			Debug.LogFormat("[SocialProcessor] User logout. User ID: {0}.", m_User.UserId);
			m_User = null;
			m_SignalBus.Fire<LogoutSignal>();
		}
	}

	async Task AppleAuth(string _Token, string _Nonce)
	{
		Credential credential = OAuthProvider.GetCredential("apple.com", _Token, _Nonce, null);
		
		FirebaseUser user = await Auth(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Apple ID failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Apple ID success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	async Task GoogleAuth(string _IDToken, string _AccessToken)
	{
		Credential credential = GoogleAuthProvider.GetCredential(_IDToken, _AccessToken);
		
		FirebaseUser user = await Auth(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Google ID failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Google ID success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	async Task<FirebaseUser> Auth(Credential _Credential)
	{
		FirebaseUser user;
		if (m_User != null)
		{
			try
			{
				user = await m_User.LinkWithCredentialAsync(_Credential);
			}
			catch (FirebaseException exception)
			{
				Debug.LogWarningFormat("[SocialProcessor] Link account failed. Error: {0}.", exception.Message);
				
				user = await m_Auth.SignInWithCredentialAsync(_Credential);
			}
		}
		else
		{
			user = await m_Auth.SignInWithCredentialAsync(_Credential);
		}
		
		m_User = user;
		
		return user;
	}
}