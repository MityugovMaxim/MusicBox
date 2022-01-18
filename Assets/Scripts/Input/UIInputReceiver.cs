using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UIInputReceiver : Graphic, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	public override bool raycastTarget => true;

	UIInputZone m_InputZone;

	readonly UIVertex[] m_Vertices =
	{
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
	};

	readonly Dictionary<int, Rect>           m_Pointers        = new Dictionary<int, Rect>();
	readonly Dictionary<UIHandle, List<int>> m_Selection       = new Dictionary<UIHandle, List<int>>();
	readonly List<UIHandle>                  m_InactiveHandles = new List<UIHandle>();
	readonly List<UIHandle>                  m_ActiveHandles   = new List<UIHandle>();

	bool m_Processing;

	[Inject]
	public void Construct(UIInputZone _InputZone)
	{
		m_InputZone = _InputZone;
	}

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

	public void RegisterIndicator(UIIndicator _Indicator)
	{
		if (_Indicator == null || _Indicator.Handle == null)
			return;
		
		UIHandle handle = _Indicator.Handle;
		
		if (m_Selection.ContainsKey(handle))
			m_Selection.Remove(handle);
		
		handle.StopReceiveInput();
		
		m_InactiveHandles.Add(handle);
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
			{
				handle.TouchDown(pointerID, area);
				break;
			}
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
		Rect rect = m_InputZone.GetWorldRect();
		
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
		for (int i = m_InactiveHandles.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_InactiveHandles[i];
			
			if (handle == null)
			{
				m_InactiveHandles.RemoveAt(i);
				continue;
			}
			
			if (!m_InputZone.RectTransform.Intersects(handle.RectTransform))
				continue;
			
			handle.StartReceiveInput();
			
			m_ActiveHandles.Add(handle);
			
			m_InactiveHandles.RemoveAt(i);
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
			
			if (m_InputZone.RectTransform.Intersects(handle.RectTransform))
				continue;
			
			handle.StopReceiveInput();
			
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