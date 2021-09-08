using System;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;
using Zenject;

[Preserve]
public class SocialProcessor : IInitializable
{
	void IInitializable.Initialize()
	{
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
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

	public void ShowAchievements()
	{
		Social.ShowAchievementsUI();
	}

	public void ShowLeaderboard(string _LeaderboardID)
	{
		GameCenterPlatform.ShowLeaderboardUI(_LeaderboardID, TimeScope.AllTime);
	}

	public void ReportScore(string _LeaderboardID, long _Score)
	{
		Social.ReportScore(
			_Score,
			_LeaderboardID,
			_Success =>
			{
				if (_Success)
					Debug.LogFormat("[SocialProcessor] Report score success. Leaderboard: '{0}'. Score: {1}.", _LeaderboardID, _Score);
				else
					Debug.LogErrorFormat("[SocialProcessor] Report score failed. Leaderboard: '{0}' Score: {1}.", _LeaderboardID, _Score);
			}
		);
	}

	public void CompleteAchievement(string _AchievementID)
	{
		Social.ReportProgress(
			_AchievementID,
			100,
			_Success =>
			{
				if (_Success)
					Debug.LogFormat("[SocialProcessor] Complete achievement success. Achievement: '{0}'.", _AchievementID);
				else
					Debug.LogErrorFormat("[SocialProcessor] Progress achievement failed. Achievement: '{0}'.",_AchievementID);
			}
		);
	}

	public void ProgressAchievement(string _AchievementID, double _Progress)
	{
		Social.ReportProgress(
			_AchievementID,
			_Progress,
			_Success =>
			{
				if (_Success)
					Debug.LogFormat("[SocialProcessor] Progress achievement success. Achievement: '{0}'. Progress: {1}.", _AchievementID, _Progress);
				else
					Debug.LogErrorFormat("[SocialProcessor] Progress achievement failed. Achievement: '{0}'. Progress: {1}.", _AchievementID, _Progress);
			}
		);
	}

	static async void GameCenterAuth()
	{
		Credential credential = await GameCenterAuthProvider.GetCredentialAsync();
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Game Center failed. Game Center credential invalid.");
			return;
		}
		
		FirebaseAuth auth = FirebaseAuth.DefaultInstance;
		
		FirebaseUser account = auth.CurrentUser;
		
		FirebaseUser user = account != null
			? await account.LinkWithCredentialAsync(credential)
			: await auth.SignInWithCredentialAsync(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Game Center failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Game Center success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	static async void AppleAuth(string _Token, string _Nonce)
	{
		Credential credential = OAuthProvider.GetCredential("apple.com", _Token, _Nonce, null);
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Apple ID failed. Apple sign in credential invalid.");
			return;
		}
		
		FirebaseAuth auth = FirebaseAuth.DefaultInstance;
		
		FirebaseUser account = auth.CurrentUser;
		
		FirebaseUser user = account != null
			? await account.LinkWithCredentialAsync(credential)
			: await auth.SignInWithCredentialAsync(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Apple ID failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Apple ID success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}

	static async void GoogleAuth(string _IDToken, string _AccessToken)
	{
		Credential credential = GoogleAuthProvider.GetCredential(_IDToken, _AccessToken);
		
		if (!credential.IsValid())
		{
			Debug.LogError("[SocialProcessor] Login with Google ID failed. Apple sign in credential invalid.");
			return;
		}
		
		FirebaseAuth auth = FirebaseAuth.DefaultInstance;
		
		FirebaseUser account = auth.CurrentUser;
		
		FirebaseUser user = account != null
			? await account.LinkWithCredentialAsync(credential)
			: await auth.SignInWithCredentialAsync(credential);
		
		if (user == null)
			Debug.LogError("[SocialProcessor] Login with Google ID failed. Unknown error.");
		else
			Debug.LogFormat("[SocialProcessor] Login with Google ID success. Username: {0}. User ID: {1}", user.DisplayName, user.UserId);
	}
}