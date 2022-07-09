using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class InputModule
{
	public event Action<int, Vector2> OnPointerDown;
	public event Action<int, Vector2> OnPointerMove;
	public event Action<int, Vector2> OnPointerUp;
	public event Action<int, Vector2> OnPointerCancel;

	readonly Camera                   m_Camera;
	readonly RectTransform            m_RectTransform;
	readonly HashSet<int>             m_PointerIDs;
	readonly Dictionary<int, Vector2> m_Positions;

	public InputModule(Camera _Camera, RectTransform _RectTransform)
	{
		m_Camera        = _Camera;
		m_RectTransform = _RectTransform;
		m_PointerIDs    = new HashSet<int>();
		m_Positions     = new Dictionary<int, Vector2>();
	}

	public void Process()
	{
		#if UNITY_EDITOR
		Vector2 position = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
			ProcessTouchDown(0, position);
		if (Input.GetMouseButton(0))
			ProcessTouchMove(0, m_Positions[0] - position);
		if (Input.GetMouseButtonUp(0))
			ProcessTouchUp(0, position);
		if (!Input.GetMouseButton(0))
			ProcessTouchCancel(0, position);
		#else
		for (int i = 0; i < Input.touchCount; i++)
			ProcessTouch(i);
		#endif
	}

	bool ContainsPointer(Vector2 _Position)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(
			m_RectTransform,
			_Position,
			m_Camera
		);
	}

	Vector2 GetLocalPosition(Vector2 _Position)
	{
		RectTransformUtility.ScreenPointToWorldPointInRectangle(
			m_RectTransform,
			_Position,
			m_Camera,
			out Vector3 position
		);
		return position;
	}

	#if !UNITY_EDITOR
	void ProcessTouch(int _PointerID)
	{
		Touch touch = Input.GetTouch(_PointerID);
		
		switch (touch.phase)
		{
			case TouchPhase.Began:
				ProcessTouchDown(touch.fingerId, touch.position);
				break;
			
			case TouchPhase.Moved:
				ProcessTouchMove(touch.fingerId, touch.deltaPosition);
				break;
			
			case TouchPhase.Ended:
				ProcessTouchCancel(touch.fingerId, touch.position);
				break;
			
			case TouchPhase.Canceled:
				ProcessTouchCancel(touch.fingerId, touch.position);
				break;
		}
	}
	#endif

	void ProcessTouchDown(int _PointerID, Vector2 _Position)
	{
		if (OnPointerDown == null || !ContainsPointer(_Position) || m_PointerIDs.Contains(_PointerID))
			return;
		
		m_PointerIDs.Add(_PointerID);
		m_Positions[_PointerID] = _Position;
		
		OnPointerDown.Invoke(_PointerID, GetLocalPosition(_Position));
	}

	void ProcessTouchMove(int _PointerID, Vector2 _Delta)
	{
		if (OnPointerMove == null || !m_PointerIDs.Contains(_PointerID))
			return;
		
		Vector2 position = m_Positions[_PointerID] + _Delta;
		
		m_Positions[_PointerID] = position;
		
		OnPointerMove.Invoke(_PointerID, GetLocalPosition(position));
	}

	void ProcessTouchUp(int _PointerID, Vector2 _Position)
	{
		if (OnPointerUp == null || !m_PointerIDs.Contains(_PointerID))
			return;
		
		m_PointerIDs.Remove(_PointerID);
		
		OnPointerUp(_PointerID, GetLocalPosition(_Position));
	}

	void ProcessTouchCancel(int _PointerID, Vector2 _Position)
	{
		if (!m_PointerIDs.Contains(_PointerID))
			return;
		
		m_PointerIDs.Remove(_PointerID);
		
		Vector2 position = GetLocalPosition(_Position);
		
		OnPointerCancel?.Invoke(_PointerID, position);
		
		OnPointerUp?.Invoke(_PointerID, position);
	}
}