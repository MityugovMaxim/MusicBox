using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionChain
{
	readonly MonoBehaviour    m_Context;
	readonly List<ActionLink> m_ActionLinks;

	Action m_Finished;

	public ActionChain(MonoBehaviour _Context = null)
	{
		m_Context     = _Context;
		m_ActionLinks = new List<ActionLink>();
	}

	public ActionChain(params ActionLink[] _ActionLinks)
	{
		m_Context     = null;
		m_ActionLinks = new List<ActionLink>(_ActionLinks);
	}

	public ActionChain(MonoBehaviour _Context = null, params ActionLink[] _ActionLinks)
	{
		m_Context     = _Context;
		m_ActionLinks = new List<ActionLink>(_ActionLinks);
	}

	public void Push(ActionLink _ActionLink)
	{
		m_ActionLinks.Add(_ActionLink);
	}

	public void Insert(params ActionLink[] _ActionLinks)
	{
		m_ActionLinks.InsertRange(0, _ActionLinks);
	}

	public void Clear()
	{
		m_ActionLinks.Clear();
	}

	public void Execute(Action _Finished = null)
	{
		m_Finished = _Finished;
		
		ProcessLinks();
	}

	public void StartCoroutine(IEnumerator _Routine)
	{
		if (m_Context == null)
		{
			Debug.LogError("[ActionChain] Start coroutine failed. Context is null.");
			return;
		}
		
		m_Context.StartCoroutine(_Routine);
	}

	public void StopCoroutine(IEnumerator _Routine)
	{
		if (m_Context == null)
		{
			Debug.LogError("[ActionChain] Stop coroutine failed. Context is null.");
			return;
		}
		
		m_Context.StopCoroutine(_Routine);
	}

	void ProcessLinks()
	{
		while (m_ActionLinks.Count > 0)
		{
			ActionLink actionLink = m_ActionLinks[0];
			
			m_ActionLinks.RemoveAt(0);
			
			if (actionLink == null)
			{
				Debug.LogWarning("[ActionChain] Execute warning. Action link is null.");
				continue;
			}
			
			actionLink.Execute(this, ProcessLinks);
			
			return;
		}
		
		m_Finished?.Invoke();
	}
}