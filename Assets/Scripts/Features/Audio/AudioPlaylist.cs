using System.Collections.Generic;
using UnityEngine;

public class AudioPlaylist
{
	public int Length => m_Items.Count;

	readonly List<AudioTrack> m_Items = new List<AudioTrack>();

	public void Add(AudioTrack _Item)
	{
		m_Items.Add(_Item);
	}

	public void Remove(AudioTrack _Item)
	{
		m_Items.Remove(_Item);
	}

	public string GetID(int _Index)
	{
		AudioTrack item = GetTrack(_Index);
		
		return item?.ID ?? string.Empty;
	}

	public string GetTitle(int _Index)
	{
		AudioTrack item = GetTrack(_Index);
		
		return item?.Title ?? string.Empty;
	}

	public string GetArtist(int _Index)
	{
		AudioTrack item = GetTrack(_Index);
		
		return item?.Artist ?? string.Empty;
	}

	public string GetSound(int _Index)
	{
		AudioTrack item = GetTrack(_Index);
		
		return item?.Sound ?? string.Empty;
	}

	public void Clear()
	{
		m_Items.Clear();
	}

	public void Shuffle()
	{
		for (int i = 0; i < m_Items.Count; i++)
		{
			int j = Random.Range(i, m_Items.Count);
			
			(m_Items[i], m_Items[j]) = (m_Items[j], m_Items[i]);
		}
	}

	public AudioTrack GetTrack(int _Index)
	{
		if (m_Items.Count == 0)
			return null;
		
		int index = MathUtility.Repeat(_Index, m_Items.Count);
		
		return m_Items[index];
	}
}