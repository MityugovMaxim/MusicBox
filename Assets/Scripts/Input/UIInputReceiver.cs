using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class UIInputReceiver : UIEntity, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[SerializeField] RectTransform m_InputArea;

	float InputError  { get; set; }
	float InputOffset { get; set; }

	[Inject] SignalBus       m_SignalBus;
	[Inject] ConfigProcessor m_ConfigProcessor;

	readonly Dictionary<int, Rect>           m_Pointers        = new Dictionary<int, Rect>();
	readonly Dictionary<UIHandle, List<int>> m_Selection       = new Dictionary<UIHandle, List<int>>();
	readonly List<UIHandle>                  m_InactiveHandles = new List<UIHandle>();
	readonly List<UIHandle>                  m_ActiveHandles   = new List<UIHandle>();

	bool m_Processing;

	public void Process()
	{
		if (m_Processing)
			return;
		
		m_Processing = true;
		
		EnableHandles();
		
		MoveInput();
		
		DisableHandles();
		
		m_Processing = false;
	}

	public void Release()
	{
		Rect area = m_InputArea.GetWorldRect();
		
		foreach (int pointerID in m_Pointers.Keys)
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (DeselectHandle(handle, pointerID))
				handle.TouchUp(pointerID, area);
		}
		
		m_Pointers.Clear();
	}

	public void RegisterIndicator(UIIndicator _Indicator)
	{
		if (_Indicator == null || _Indicator.Handle == null)
			return;
		
		UIHandle handle = _Indicator.Handle;
		
		if (m_Selection.ContainsKey(handle))
			m_Selection.Remove(handle);
		
		m_InactiveHandles.Add(handle);
	}

	public void UnregisterIndicator(UIIndicator _Indicator)
	{
		if (_Indicator == null || _Indicator.Handle == null)
			return;
		
		UIHandle handle = _Indicator.Handle;
		
		if (m_Selection.ContainsKey(handle))
			m_Selection.Remove(handle);
		
		m_InactiveHandles.Remove(handle);
		m_ActiveHandles.Remove(handle);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		int  pointerID = _EventData.pointerId;
		Rect area      = GetZoneArea(_EventData);
		
		m_Pointers[pointerID] = area;
		
		_EventData.Use();
		
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (handle.Select(area) && SelectHandle(handle, pointerID))
			{
				handle.TouchDown(pointerID, area);
				return;
			}
		}
		
		m_SignalBus.Fire<InputMissSignal>();
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		int  pointerID = _EventData.pointerId;
		Rect area      = GetZoneArea(_EventData);
		
		m_Pointers.Remove(pointerID);
		
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (DeselectHandle(handle, pointerID))
				handle.TouchUp(pointerID, area);
		}
		
		_EventData.Use();
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		int  pointerID = _EventData.pointerId;
		Rect area      = GetZoneArea(_EventData);
		
		m_Pointers[pointerID] = area;
		
		_EventData.Use();
	}

	Rect GetAreaRect()
	{
		Rect rect = m_InputArea.GetWorldRect();
		
		RectOffset padding = new RectOffset(
			0,
			0,
			(int)(InputError + InputOffset),
			(int)InputError
		);
		
		return padding.Add(rect);
	}

	Rect GetZoneArea(PointerEventData _EventData)
	{
		Rect rect = GetAreaRect();
		
		Vector2 position = new Vector2(
			_EventData.pointerCurrentRaycast.worldPosition.x,
			rect.y + rect.height * 0.5f
		);
		
		Vector2 size = new Vector2(0, rect.height);
		
		return new Rect(position - size * 0.5f, size);
	}

	List<int> GetPointerIDs(UIHandle _Handle)
	{
		return m_Selection.ContainsKey(_Handle) ? m_Selection[_Handle] : null;
	}

	void EnableHandles()
	{
		Rect rect = GetAreaRect();
		
		float enterThreshold = rect.yMax;
		float exitThreshold  = rect.yMin;
		
		for (int i = m_InactiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_InactiveHandles[i];
			
			if (handle == null)
			{
				m_InactiveHandles.RemoveAt(i);
				continue;
			}
			
			Rect handleRect = handle.GetWorldRect();
			
			if (handleRect.yMin > enterThreshold)
				continue;
			
			if (handleRect.yMax < exitThreshold)
			{
				handle.ExitZone();
				m_InactiveHandles.RemoveAt(i);
				continue;
			}
			
			handle.EnterZone();
			m_ActiveHandles.Add(handle);
			m_InactiveHandles.RemoveAt(i);
		}
	}

	void DisableHandles()
	{
		Rect rect = GetAreaRect();
		
		float enterThreshold = rect.yMax;
		float exitThreshold  = rect.yMin;
		
		for (int i = m_ActiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_ActiveHandles[i];
			
			if (handle == null)
			{
				m_ActiveHandles.RemoveAt(i);
				continue;
			}
			
			Rect handleRect = handle.GetWorldRect();
			
			if (handleRect.yMin > enterThreshold)
			{
				handle.Reverse();
				m_ActiveHandles.RemoveAt(i);
				m_InactiveHandles.Insert(0, handle);
				continue;
			}
			
			if (handleRect.yMax > exitThreshold)
				continue;
			
			handle.ExitZone();
			m_ActiveHandles.RemoveAt(i);
		}
	}

	void MoveInput()
	{
		for (int i = m_ActiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_ActiveHandles[i];
			
			if (handle == null)
				continue;
			
			List<int> pointerIDs = GetPointerIDs(handle);
			
			if (pointerIDs == null)
				continue;
			
			foreach (int pointerID in pointerIDs)
			{
				if (!m_Pointers.ContainsKey(pointerID))
					continue;
				
				Rect area = m_Pointers[pointerID];
				
				handle.TouchMove(pointerID, area);
			}
		}
	}

	bool SelectHandle(UIHandle _Handle, int _PointerID)
	{
		if (!m_Selection.ContainsKey(_Handle))
			m_Selection[_Handle] = new List<int>();
		
		if (m_Selection[_Handle].Contains(_PointerID))
		{
			Debug.LogError($"[UIInputReceiver] Select handle failed. Handle '{_Handle.name}' already selected by pointer '{_PointerID}'", _Handle.gameObject);
			return false;
		}
		
		m_Selection[_Handle].Add(_PointerID);
		
		return true;
	}

	bool DeselectHandle(UIHandle _Handle, int _PointerID)
	{
		if (!m_Selection.ContainsKey(_Handle) || !m_Selection[_Handle].Contains(_PointerID))
			return false;
		
		m_Selection[_Handle].Remove(_PointerID);
		
		if (m_Selection[_Handle].Count == 0)
			m_Selection.Remove(_Handle);
		
		return true;
	}
}