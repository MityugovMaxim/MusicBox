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
		Social.localUser.Authenticate(
			(_Success, _Error) =>
			{
				if (_Success)
					Debug.LogFormat("[SocialProcessor] Authenticate success. User ID: {0}. User name: {1}.", Social.localUser.id, Social.localUser.userName);
				else
					Debug.LogErrorFormat("[SocialProcessor] Authenticate failed. Error: {0}.", _Error);
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
}