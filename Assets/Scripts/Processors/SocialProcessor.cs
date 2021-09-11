using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.SocialPlatforms.GameCenter;
using Zenject;

[Preserve]
public class LoginSignal { }

[Preserve]
public class LogoutSignal { }

[Preserve]
public class SocialProcessor : IInitializable, IDisposable
{
	public string UserID => m_User?.UserId;
	public string Email  => m_User?.Email;
	public string Name   => m_User?.DisplayName;
	public Uri    Photo  => m_User?.PhotoUrl;

	readonly SignalBus m_SignalBus;

	FirebaseAuth m_Auth;
	FirebaseUser m_User;

	[Inject]
	public SocialProcessor(SignalBus _SignalBus)
	{
		m_SignalBus = _SignalBus;
	}

	public async Task Login()
	{
		if (m_User == null)
			await m_Auth.SignInAnonymouslyAsync();
		
		if (m_User != null)
			await m_User.ReloadAsync();
	}

	public void AttachGameCenter()
	{
		Social.localUser.Authenticate(
			(_Success, _Error) =>
			{
				if (_Success)
				{
					Debug.LogFormat("[SocialManager] Login with Apple ID success. Player ID: {0}.", Social.localUser.id);
					
					GameCenterAuth();
				}
				else
				{
					Debug.LogErrorFormat("[SocialManager] Login with Game Center failed. Error: {0}.", _Error);
				}
			}
		);
	}

	public void AttachAppleID()
	{
		string nonce = Guid.NewGuid().ToString();
		
		AppleAuthManager.Login(
			nonce,
			_Token =>
			{
				Debug.LogFormat("[SocialManager] Login with Apple ID success. Nonce: {0}. Token: {1}.", nonce, _Token);
				
				AppleAuth(_Token, nonce);
			},
			_Error =>
			{
				Debug.LogErrorFormat("[SocialManager] Login with Apple ID failed. Nonce: {0}. Error: {1}.", nonce, _Error);
			}
		);
	}

	public void AttachGoogleID()
	{
		GoogleAuthManager.Login(
			"266200973318-r1sbm8gud1amf7rvd04u8shn2mqkt3ci.apps.googleusercontent.com",
			_Token =>
			{
				Debug.LogFormat("[SocialManager] Login with Google ID success. Token: {0}.", _Token);
				
				GoogleAuth(_Token, null);
			},
			_Error =>
			{
				Debug.LogErrorFormat("[SocialManager] Login with Apple ID failed. Error: {0}.", _Error);
			}
		);
	}

	void IInitializable.Initialize()
	{
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
		
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

	async void GameCenterAuth()
	{
		Credential credential = await GameCenterAuthProvider.GetCredentialAsync();
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Game Center failed. Game Center credential invalid.");
			return;
		}
		
		FirebaseUser user = await Auth(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Game Center failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Game Center success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	async void AppleAuth(string _Token, string _Nonce)
	{
		Credential credential = OAuthProvider.GetCredential("apple.com", _Token, _Nonce, null);
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Apple ID failed. Apple sign in credential invalid.");
			return;
		}
		
		FirebaseUser user = await Auth(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Apple ID failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Apple ID success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	async void GoogleAuth(string _IDToken, string _AccessToken)
	{
		Credential credential = GoogleAuthProvider.GetCredential(_IDToken, _AccessToken);
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Google ID failed. Apple sign in credential invalid.");
			return;
		}
		
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