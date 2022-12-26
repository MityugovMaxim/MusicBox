using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

[Menu(MenuType.TransitionMenu)]
public class UITransitionMenu : UIAnimationMenu
{
	[SerializeField] UIDisc[] m_Discs;
	[SerializeField] UIFlare  m_Flare;

	[SerializeField, Sound] string m_Sound;

	[Inject] SoundProcessor m_SoundProcessor;

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		
		if (m_Flare != null)
			m_Flare.Play();
		
		ProcessDiscs();
	}

	protected override void OnShowFinished()
	{
		base.OnShowFinished();
		
		Hide(true);
	}

	[UsedImplicitly]
	void PlaySound()
	{
		if (m_SoundProcessor != null && !string.IsNullOrEmpty(m_Sound))
			m_SoundProcessor.Play(m_Sound);
	}

	[ContextMenu("Process discs")]
	void ProcessDiscs()
	{
		RankType[] ranks = Enum.GetValues(typeof(RankType))
			.OfType<RankType>()
			.Where(_Rank => _Rank != RankType.None)
			.ToArray();
		
		foreach (UIDisc disc in m_Discs)
		{
			if (disc == null)
				continue;
			
			int index = Random.Range(0, ranks.Length);
			
			disc.Rank = ranks[index];
		}
	}
}
