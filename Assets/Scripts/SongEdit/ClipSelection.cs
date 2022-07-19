using System;
using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;

public static class ClipSelection
{
	public static event Action Changed;

	static readonly HashSet<ASFClip> m_Items = new HashSet<ASFClip>();

	public static void Select(ASFClip _Item)
	{
		m_Items.Add(_Item);
		
		Changed?.Invoke();
	}

	public static void Deselect(ASFClip _Item)
	{
		m_Items.Remove(_Item);
		
		Changed?.Invoke();
	}

	public static T[] GetItems<T>() where T : ASFClip
	{
		return m_Items.OfType<T>().ToArray();
	}

	public static bool Contains(ASFClip _Item)
	{
		return m_Items.Contains(_Item);
	}

	public static void Clear()
	{
		m_Items.Clear();
		
		Changed?.Invoke();
	}
}
