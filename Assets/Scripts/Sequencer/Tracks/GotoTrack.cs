using System;
using UnityEngine;

#if UNITY_EDITOR
public partial class GotoTrack
{
	protected override float MinHeight => 50;
	protected override float MaxHeight => 50;
}
#endif

[CreateAssetMenu(fileName = "Goto Track", menuName = "Tracks/Goto Track")]
public partial class GotoTrack : Track<GotoClip>
{
	GotoResolver Resolver
	{
		get
		{
			if (m_ResolverReference == null || m_ResolverCache != m_Resolver)
			{
				m_ResolverReference = GetReference<GotoResolver>(m_Resolver);
				m_ResolverCache     = m_Resolver;
			}
			return m_ResolverReference;
		}
	}

	[SerializeField, Reference(typeof(GotoResolver))] string m_Resolver;

	[NonSerialized] string       m_ResolverCache;
	[NonSerialized] GotoResolver m_ResolverReference;

	public override void Initialize(Sequencer _Sequencer)
	{
		base.Initialize(_Sequencer);
		
		if (Resolver == null)
		{
			Debug.LogError($"[GotoTrack] Resolver is not assigned at '{name}'.", this);
			return;
		}
		
		foreach (GotoClip clip in Clips)
			clip.Initialize(Sequencer, Resolver);
	}
}