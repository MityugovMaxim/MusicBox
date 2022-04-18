using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongRating : UIEntity
{
	[SerializeField] Toggle m_LikeButton;
	[SerializeField] Toggle m_DislikeButton;

	[Inject] ScoresProcessor m_ScoresProcessor;
	[Inject] HapticProcessor m_HapticProcessor;

	string m_SongID;
	int    m_SourceRating;
	int    m_TargetRating;

	public void Setup(string _SongID)
	{
		m_SongID       = _SongID;
		m_SourceRating = m_ScoresProcessor.GetRating(m_SongID);
		m_TargetRating = m_ScoresProcessor.GetRating(m_SongID);
		
		m_LikeButton.SetIsOnWithoutNotify(m_SourceRating == 1);
		m_DislikeButton.SetIsOnWithoutNotify(m_SourceRating == -1);
	}

	public void Like(bool _Value)
	{
		m_TargetRating = _Value ? 1 : 0;
		
		Haptic.Type haptic = _Value ? Haptic.Type.Success : Haptic.Type.Selection;
		
		m_HapticProcessor.Process(haptic);
	}

	public void Dislike(bool _Value)
	{
		m_TargetRating = _Value ? -1 : 0;
		
		m_HapticProcessor.Process(Haptic.Type.Selection);
	}

	public async void Execute()
	{
		if (m_SourceRating == m_TargetRating)
			return;
		
		SongRatingRequest request = new SongRatingRequest(m_SongID, m_TargetRating);
		
		await request.SendAsync();
	}
}