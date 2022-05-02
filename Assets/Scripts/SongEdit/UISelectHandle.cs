using System.Collections.Generic;
using System.Linq;
using AudioBox.ASF;
using UnityEngine;
using Zenject;

public class UISelectHandle : UIEntity
{
	[SerializeField] RectTransform[] m_Ignore;

	[Inject] UIPlayer m_Player;

	Vector2 m_Position;

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			m_Position = Input.mousePosition;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			if (CheckIgnore(Input.mousePosition))
				return;
			
			float distance = Vector2.Distance(Input.mousePosition, m_Position);
			
			if (distance > 5)
				return;
			
			List<ASFClip> clips = GetClips(Input.mousePosition);
			
			if (clips == null || clips.Count == 0)
			{
				ClipSelection.Clear();
				return;
			}
			
			foreach (ASFClip clip in clips)
			{
				if (ClipSelection.Contains(clip))
					ClipSelection.Deselect(clip);
				else
					ClipSelection.Select(clip);
			}
		}
	}

	bool CheckIgnore(Vector2 _Position)
	{
		return m_Ignore != null && m_Ignore.Any(_Ignore => ContainsPoint(_Ignore, _Position));
	}

	List<ASFClip> GetClips(Vector2 _Position)
	{
		return new List<ASFClip>()
			.Union(GetTapClips(_Position))
			.Union(GetDoubleClips(_Position))
			.Union(GetHoldClips(_Position))
			.Union(GetColorClips(_Position))
			.ToList();
	}

	List<ASFTapClip> GetTapClips(Vector2 _Position)
	{
		UITapClipContext[] contexts = m_Player.GetComponentsInChildren<UITapClipContext>();
		
		return contexts
			.Where(_Context => ContainsPoint(_Context, _Position))
			.Select(_Context => _Context.Clip)
			.ToList();
	}

	List<ASFDoubleClip> GetDoubleClips(Vector2 _Position)
	{
		UIDoubleClipContext[] contexts = m_Player.GetComponentsInChildren<UIDoubleClipContext>();
		
		return contexts
			.Where(_Context => ContainsPoint(_Context, _Position))
			.Select(_Context => _Context.Clip)
			.ToList();
	}

	List<ASFHoldClip> GetHoldClips(Vector2 _Position)
	{
		UIHoldClipContext[] contexts = m_Player.GetComponentsInChildren<UIHoldClipContext>();
		
		return contexts
			.Where(_Context => ContainsPoint(_Context, _Position))
			.Where(_Context => _Context.ContainsPoint(_Position))
			.Select(_Context => _Context.Clip)
			.ToList();
	}

	List<ASFColorClip> GetColorClips(Vector2 _Position)
	{
		UIColorClipContext[] contexts = m_Player.GetComponentsInChildren<UIColorClipContext>();
		
		return contexts.Where(_Context => ContainsPoint(_Context, _Position))
			.Select(_Context => _Context.Clip)
			.ToList();
	}

	static bool ContainsPoint(UIEntity _Entity, Vector2 _Position)
	{
		return ContainsPoint(_Entity.RectTransform, _Position);
	}

	static bool ContainsPoint(RectTransform _RectTransform, Vector2 _Position)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(_RectTransform, _Position);
	}
}