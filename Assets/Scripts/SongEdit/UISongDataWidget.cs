using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UISongDataWidget : UISongWidget
{
	[SerializeField] Button m_CopyButton;
	[SerializeField] Button m_PasteButton;
	[SerializeField] Button m_DeleteButton;
	[SerializeField] Button m_CleanupButton;

	[Inject] UIPlayer m_Player;
	[Inject] UIBeat   m_Beat;

	readonly List<ASFClip> m_Buffer = new List<ASFClip>();

	protected override void Awake()
	{
		base.Awake();
		
		m_CopyButton.Subscribe(Copy);
		m_PasteButton.Subscribe(Paste);
		m_DeleteButton.Subscribe(Delete);
		m_CleanupButton.Subscribe(Cleanup);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CopyButton.Unsubscribe(Copy);
		m_PasteButton.Unsubscribe(Paste);
		m_DeleteButton.Unsubscribe(Delete);
		m_CleanupButton.Unsubscribe(Cleanup);
	}

	void Copy()
	{
		if (Expand())
			return;
		
		m_Buffer.Clear();
		CopyClips<ASFTapClip>();
		CopyClips<ASFDoubleClip>();
		CopyClips<ASFHoldClip>();
		CopyClips<ASFColorClip>();
	}

	void Paste()
	{
		if (Expand())
			return;
		
		PasteClips<ASFTapTrack, ASFTapClip>();
		PasteClips<ASFDoubleTrack, ASFDoubleClip>();
		PasteClips<ASFHoldTrack, ASFHoldClip>();
		PasteClips<ASFColorTrack, ASFColorClip>();
		
		m_Player.Sample();
	}

	void Delete()
	{
		if (Expand())
			return;
		
		DeleteClips<ASFTapTrack, ASFTapClip>();
		DeleteClips<ASFDoubleTrack, ASFDoubleClip>();
		DeleteClips<ASFHoldTrack, ASFHoldClip>();
		DeleteClips<ASFColorTrack, ASFColorClip>();
		ClipSelection.Clear();
		
		m_Player.Sample();
	}

	void Cleanup()
	{
		if (Expand())
			return;
		
		m_Player.Cleanup();
		
		m_Player.Sample();
	}

	void DeleteClips<TTrack, TClip>() where TTrack : ASFTrack<TClip> where TClip : ASFClip
	{
		TClip[] clips = ClipSelection.GetItems<TClip>();
		if (clips == null)
			return;
		
		TTrack track = m_Player.GetTrack<TTrack>();
		if (track == null)
			return;
		
		foreach (TClip clip in clips)
			track.RemoveClip(clip);
	}

	void CopyClips<TClip>() where TClip : ASFClip
	{
		TClip[] clips = ClipSelection.GetItems<TClip>();
		
		if (clips == null)
			return;
		
		foreach (TClip clip in clips)
			m_Buffer.Add(clip.Clone());
	}

	void PasteClips<TTrack, TClip>() where TTrack : ASFTrack<TClip> where TClip : ASFClip
	{
		if (m_Buffer.Count == 0)
			return;
		
		TTrack track = m_Player.GetTrack<TTrack>();
		
		if (track == null)
			return;
		
		double time   = m_Beat.Snap(m_Player.Time);
		double offset = m_Buffer.Min(_Clip => _Clip.MinTime);
		
		TClip[] clips = m_Buffer.OfType<TClip>().ToArray();
		
		foreach (TClip clip in clips)
		{
			TClip clone = clip.Clone() as TClip;
			
			if (clone == null)
				continue;
			
			clone.MinTime -= offset;
			clone.MaxTime -= offset;
			clone.MinTime += time;
			clone.MaxTime += time;
			track.AddClip(clone);
		}
		
		track.SortClips();
	}
}
