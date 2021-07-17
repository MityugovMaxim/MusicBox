using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputReceiver : Graphic, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	[Serializable]
	public class IndicatorEvent : UnityEvent { }

	[Serializable]
	public class HandleEvent : UnityEvent<float> { }

	public RectTransform Zone => m_Zone;

	public override bool raycastTarget => true;

	[SerializeField] RectTransform  m_Zone;
	[SerializeField] HandleEvent    m_OnSuccess;
	[SerializeField] HandleEvent    m_OnFail;
	[SerializeField] IndicatorEvent m_OnHit;
	[SerializeField] IndicatorEvent m_OnMiss;

	readonly UIVertex[] m_Vertices =
	{
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
	};

	readonly Dictionary<int, Rect>           m_Pointers           = new Dictionary<int, Rect>();
	readonly Dictionary<UIHandle, List<int>> m_Selection          = new Dictionary<UIHandle, List<int>>();
	readonly List<UIHandle>                  m_InactiveHandles    = new List<UIHandle>();
	readonly List<UIHandle>                  m_ActiveHandles      = new List<UIHandle>();
	readonly List<UIIndicator>               m_InactiveIndicators = new List<UIIndicator>();
	readonly List<UIIndicator>               m_ActiveIndicators   = new List<UIIndicator>();

	public void Process()
	{
		EnableIndicators();
		
		EnableHandles();
		
		MoveInput();
		
		DisableHandles();
		
		DisableIndicators();
	}

	public void RegisterIndicator(UIIndicator _Indicator)
	{
		if (_Indicator == null || _Indicator.Handle == null)
			return;
		
		UIHandle handle = _Indicator.Handle;
		
		if (m_Selection.ContainsKey(handle))
			m_Selection.Remove(handle);
		
		handle.StopReceiveInput();
		
		m_InactiveHandles.Add(handle);
		
		m_InactiveIndicators.Add(_Indicator);
	}

	public void UnregisterIndicator(UIIndicator _Indicator)
	{
		if (_Indicator == null || _Indicator.Handle == null)
			return;
		
		UIHandle handle = _Indicator.Handle;
		
		if (m_Selection.ContainsKey(handle))
			m_Selection.Remove(handle);
		
		handle.StopReceiveInput();
		
		m_InactiveHandles.Remove(handle);
		m_ActiveHandles.Remove(handle);
		
		m_ActiveIndicators.Remove(_Indicator);
		m_InactiveIndicators.Remove(_Indicator);
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		Rect rect = GetPixelAdjustedRect();
		
		m_Vertices[0].position = new Vector2(rect.xMin, rect.yMin);
		m_Vertices[1].position = new Vector2(rect.xMin, rect.yMax);
		m_Vertices[2].position = new Vector2(rect.xMax, rect.yMax);
		m_Vertices[3].position = new Vector2(rect.xMax, rect.yMin);
		
		_VertexHelper.Clear();
		_VertexHelper.AddUIVertexQuad(m_Vertices);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		int  pointerID = _EventData.pointerId;
		Rect area      = GetZoneArea(_EventData);
		
		m_Pointers[pointerID] = area;
		
		foreach (UIHandle handle in m_ActiveHandles)
		{
			if (handle == null)
				continue;
			
			if (handle.Select(area) && SelectHandle(handle, pointerID))
				handle.TouchDown(pointerID, area);
		}
		
		_EventData.Use();
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

	Rect GetZoneArea(PointerEventData _EventData)
	{
		Rect rect = m_Zone.GetWorldRect();
		
		Vector2 position = new Vector2(
			_EventData.pointerCurrentRaycast.worldPosition.x,
			rect.y + rect.height * 0.5f
		);
		
		Vector2 size = new Vector2(
			EventSystem.current.pixelDragThreshold,
			rect.height
		);
		
		return new Rect(position - size * 0.5f, size);
	}

	List<int> GetPointerIDs(UIHandle _Handle)
	{
		return m_Selection.ContainsKey(_Handle) ? m_Selection[_Handle] : null;
	}

	void EnableIndicators()
	{
		for (int i = m_InactiveIndicators.Count - 1; i >= 0; i--)
		{
			UIIndicator indicator = m_InactiveIndicators[i];
			
			if (indicator == null)
			{
				m_InactiveHandles.RemoveAt(i);
				continue;
			}
			
			if (!m_Zone.Intersects(indicator.RectTransform))
				continue;
			
			m_InactiveIndicators.RemoveAt(i);
			m_ActiveIndicators.Add(indicator);
		}
	}

	void DisableIndicators()
	{
		for (int i = m_ActiveIndicators.Count - 1; i >= 0; i--)
		{
			UIIndicator indicator = m_ActiveIndicators[i];
			
			if (indicator == null)
			{
				m_ActiveIndicators.RemoveAt(i);
				continue;
			}
			
			if (m_Zone.Intersects(indicator.RectTransform))
				continue;
			
			m_ActiveIndicators.RemoveAt(i);
			
			UIHandle handle = indicator.Handle;
			
			if (handle == null)
				continue;
			
			if (Mathf.Approximately(handle.Progress, 0))
				m_OnMiss?.Invoke();
		}
	}

	void EnableHandles()
	{
		for (int i = m_InactiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_InactiveHandles[i];
			
			if (handle == null)
			{
				m_InactiveHandles.RemoveAt(i);
				continue;
			}
			
			if (!m_Zone.Intersects(handle.RectTransform))
				continue;
			
			handle.StartReceiveInput();
			
			m_ActiveHandles.Add(handle);
			
			m_InactiveHandles.RemoveAt(i);
			
			handle.OnSuccess += m_OnSuccess.Invoke;
			handle.OnFail    += m_OnFail.Invoke;
		}
	}

	void DisableHandles()
	{
		for (int i = m_ActiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_ActiveHandles[i];
			
			if (handle == null)
			{
				m_ActiveHandles.RemoveAt(i);
				continue;
			}
			
			if (m_Zone.Intersects(handle.RectTransform))
				continue;
			
			handle.StopReceiveInput();
			
			m_ActiveHandles.RemoveAt(i);
			
			handle.OnSuccess -= m_OnSuccess.Invoke;
			handle.OnFail    -= m_OnFail.Invoke;
		}
	}

	void MoveInput()
	{
		for (int i = m_ActiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_ActiveHandles[i];
			
			if (handle == null)
			{
				m_ActiveHandles.RemoveAt(i);
				continue;
			}
			
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
		
		m_OnHit?.Invoke();
		
		return true;
	}

	bool DeselectHandle(UIHandle _Handle, int _PointerID)
	{
		if (!m_Selection.ContainsKey(_Handle))
			return false;
		
		if (!m_Selection[_Handle].Contains(_PointerID))
		{
			Debug.LogError($"[UIInputReceiver] Deselect handle failed. Handle '{_Handle.name}' is not selected by pointer '{_PointerID}'", _Handle.gameObject);
			return false;
		}
		
		m_Selection[_Handle].Remove(_PointerID);
		
		if (m_Selection[_Handle].Count == 0)
			m_Selection.Remove(_Handle);
		
		return true;
	}
}