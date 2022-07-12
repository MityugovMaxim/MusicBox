using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UIInputReceiver : UIEntity
{
	Rect Area { get; set; }

	public event Action<UIHandle> OnSelect;

	[SerializeField] Camera        m_Camera;
	[SerializeField] RectTransform m_InputArea;

	[Inject] FXProcessor     m_FXProcessor;
	[Inject] ScoreManager    m_ScoreManager;
	[Inject] ConfigProcessor m_ConfigProcessor;

	InputModule m_InputModule;

	readonly Dictionary<int, Rect>           m_Pointers        = new Dictionary<int, Rect>();
	readonly Dictionary<UIHandle, List<int>> m_Selection       = new Dictionary<UIHandle, List<int>>();
	readonly List<UIHandle>                  m_InactiveHandles = new List<UIHandle>();
	readonly List<UIHandle>                  m_ActiveHandles   = new List<UIHandle>();

	bool m_Processing;

	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		UnityEditor.Handles.DrawSolidRectangleWithOutline(
			Area,
			new Color(0, 1, 0, 0.25f),
			new Color(0, 1, 0, 0.75f)
		);
	}
	#endif

	public void Setup(float _Ratio)
	{
		float position = 1.0f - _Ratio;
		m_InputArea.anchorMin = new Vector2(0, position);
		m_InputArea.anchorMax = new Vector2(1, position);
		
		m_InputModule               =  new InputModule(m_Camera, RectTransform);
		m_InputModule.OnPointerDown += PointerDown;
		m_InputModule.OnPointerMove += PointerMove;
		m_InputModule.OnPointerUp   += PointerUp;
		
		float extend = m_ConfigProcessor.InputExtend;
		float offset = m_ConfigProcessor.InputOffset;
		
		Rect area = m_InputArea.GetWorldRect();
		
		area.y      += offset;
		area.y      -= extend;
		area.height += extend * 2;
		
		Area = area;
	}

	public void Sample()
	{
		if (m_Processing)
			return;
		
		m_Processing = true;
		
		EnableHandles();
		
		m_InputModule.Process();
		
		MoveInput();
		
		DisableHandles();
		
		m_Processing = false;
	}

	public void Release()
	{
		foreach (int pointerID in m_Pointers.Keys)
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (DeselectHandle(handle, pointerID))
				handle.TouchUp(pointerID, Area);
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

	void PointerDown(int _PointerID, Vector2 _Position)
	{
		Rect area = GetPointerArea(_Position);
		
		m_Pointers[_PointerID] = area;
		
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (handle.Select(area) && SelectHandle(_PointerID, handle))
			{
				handle.TouchDown(_PointerID, area);
				OnSelect?.Invoke(handle);
				return;
			}
		}
		
		if (m_ActiveHandles.Count <= 0 && m_InactiveHandles.Count <= 0)
			return;
		
		m_FXProcessor.Fail();
		
		m_ScoreManager.Miss();
	}

	void PointerUp(int _PointerID, Vector2 _Position)
	{
		Rect area = GetPointerArea(_Position);
		
		m_Pointers.Remove(_PointerID);
		
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (DeselectHandle(handle, _PointerID))
				handle.TouchUp(_PointerID, area);
		}
	}

	void PointerMove(int _PointerID, Vector2 _Position)
	{
		Rect area = GetPointerArea(_Position);
		
		for (int i = m_ActiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_ActiveHandles[i];
			
			if (handle == null)
				continue;
			
			List<int> pointerIDs = GetPointerIDs(handle);
			
			if (pointerIDs == null || !pointerIDs.Contains(_PointerID))
				continue;
			
			handle.TouchMove(_PointerID, area);
		}
		
		m_Pointers[_PointerID] = area;
	}

	Rect GetPointerArea(Vector2 _Position)
	{
		Vector2 position = new Vector2(
			_Position.x,
			Area.y + Area.height * 0.5f
		);
		
		Vector2 size = new Vector2(0, Area.height);
		
		return new Rect(position - size * 0.5f, size);
	}

	List<int> GetPointerIDs(UIHandle _Handle)
	{
		return m_Selection.ContainsKey(_Handle) ? m_Selection[_Handle] : null;
	}

	void EnableHandles()
	{
		float enterThreshold = Area.yMax;
		float exitThreshold  = Area.yMin;
		
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
		float enterThreshold = Area.yMax;
		float exitThreshold  = Area.yMin;
		
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

	bool SelectHandle(int _PointerID, UIHandle _Handle)
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