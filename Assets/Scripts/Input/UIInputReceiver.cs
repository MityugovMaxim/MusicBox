using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputReceiver : Graphic, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	public RectTransform Zone => m_Zone;

	public override bool raycastTarget => true;

	[SerializeField] RectTransform m_Zone;

	readonly UIVertex[] m_Vertices =
	{
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
		new UIVertex(),
	};

	readonly Dictionary<int, Vector2>        m_Pointers  = new Dictionary<int, Vector2>();
	readonly Dictionary<UIHandle, List<int>> m_Selection = new Dictionary<UIHandle, List<int>>();
	readonly List<UIHandle>                  m_Inactive  = new List<UIHandle>();
	readonly List<UIHandle>                  m_Active    = new List<UIHandle>();

	public void Process()
	{
		EnableInput();
		
		MoveInput();
		
		DisableInput();
	}

	public void RegisterHandle(UIHandle _Handle)
	{
		if (_Handle == null)
			return;
		
		if (m_Selection.ContainsKey(_Handle))
			Debug.LogError($"[UIInputReceiver] Handle '{_Handle.name}' already selected.");
		
		m_Inactive.Add(_Handle);
	}

	public void UnregisterHandle(UIHandle _Handle)
	{
		if (_Handle == null)
			return;
		
		_Handle.StopReceiveInput();
		
		if (m_Selection.ContainsKey(_Handle))
			m_Selection.Remove(_Handle);
		
		m_Inactive.Remove(_Handle);
		m_Active.Remove(_Handle);
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
		int     pointerID = _EventData.pointerId;
		Vector2 position  = _EventData.position;
		Rect    zone      = m_Zone.GetWorldRect();
		Vector2 size      = new Vector2(zone.height, zone.height);
		
		position = ProjectPosition(position);
		
		Rect area = new Rect(position - size * 0.5f, size);
		
		m_Pointers[pointerID] = position;
		
		foreach (UIHandle handle in m_Active)
		{
			if (handle == null)
				continue;
			
			if (handle.Select(area) && SelectHandle(handle, pointerID))
				handle.TouchDown(pointerID, position);
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		int     pointerID = _EventData.pointerId;
		Vector2 position  = _EventData.position;
		
		position = ProjectPosition(position);
		
		m_Pointers.Remove(pointerID);
		
		foreach (UIHandle handle in m_Active)
		{
			if (handle == null)
				continue;
			
			if (DeselectHandle(handle, pointerID))
				handle.TouchUp(pointerID, position);
		}
	}

	void IDragHandler.OnDrag(PointerEventData _EventData)
	{
		int     pointerID = _EventData.pointerId;
		Vector2 position  = _EventData.position;
		
		position = ProjectPosition(position);
		
		m_Pointers[pointerID] = position;
	}

	Vector2 ProjectPosition(Vector2 _Position)
	{
		Rect rect = m_Zone.GetWorldRect();
		
		return new Vector2(_Position.x, rect.y + rect.height * 0.5f);
	}

	List<int> GetPointerIDs(UIHandle _Handle)
	{
		return m_Selection.ContainsKey(_Handle) ? m_Selection[_Handle] : null;
	}

	void EnableInput()
	{
		for (int i = m_Inactive.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_Inactive[i];
			
			if (handle == null)
			{
				m_Inactive.RemoveAt(i);
				continue;
			}
			
			if (!rectTransform.Intersects(handle.RectTransform))
				continue;
			
			if (m_Selection.ContainsKey(handle))
				m_Selection.Remove(handle);
			
			handle.StartReceiveInput();
			
			m_Active.Add(handle);
			
			m_Inactive.RemoveAt(i);
		}
	}

	void DisableInput()
	{
		for (int i = m_Active.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_Active[i];
			
			if (handle == null)
			{
				m_Active.RemoveAt(i);
				continue;
			}
			
			if (rectTransform.Intersects(handle.RectTransform))
				continue;
			
			if (m_Selection.ContainsKey(handle))
				m_Selection.Remove(handle);
			
			handle.StopReceiveInput();
			
			m_Active.RemoveAt(i);
		}
	}

	void MoveInput()
	{
		for (int i = m_Active.Count - 1; i >= 0; i--)
		{
			UIHandle handle = m_Active[i];
			
			if (handle == null)
			{
				m_Active.RemoveAt(i);
				continue;
			}
			
			List<int> pointerIDs = GetPointerIDs(handle);
			
			if (pointerIDs == null)
				continue;
			
			foreach (int pointerID in pointerIDs)
			{
				if (!m_Pointers.ContainsKey(pointerID))
					continue;
				
				Vector2 position = m_Pointers[pointerID];
				
				handle.TouchMove(pointerID, position);
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
		if (!m_Selection.ContainsKey(_Handle))
		{
			Debug.LogError($"[UIInputReceiver] Deselect handle failed. Handle '{_Handle.name}' is not selected", _Handle.gameObject);
			return false;
		}
		
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