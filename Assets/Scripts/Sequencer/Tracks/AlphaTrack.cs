using System;
using UnityEngine;

#if UNITY_EDITOR
public partial class AlphaTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Alpha Track", menuName = "Tracks/Alpha Track")]
public partial class AlphaTrack : Track<AlphaClip>
{
	CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroupReference == null || m_CanvasGroupCache != m_CanvasGroup)
			{
				m_CanvasGroupReference = GetReference<CanvasGroup>(m_CanvasGroup);
				m_CanvasGroupCache     = m_CanvasGroup;
			}
			return m_CanvasGroupReference;
		}
	}

	[SerializeField, Reference(typeof(CanvasGroup))] string m_CanvasGroup;

	[NonSerialized] string      m_CanvasGroupCache;
	[NonSerialized] CanvasGroup m_CanvasGroupReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		if (CanvasGroup == null)
		{
			Debug.LogError($"[AlphaTrack] CanvasGroup is not assigned at '{name}'.", this);
			return;
		}
		
		foreach (AlphaClip clip in Clips)
			clip.Initialize(Sequencer, CanvasGroup);
	}
}