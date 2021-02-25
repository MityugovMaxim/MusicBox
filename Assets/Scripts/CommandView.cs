using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandView : Graphic, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] CommandItem m_SwipeLeft;
	[SerializeField] CommandItem m_SwipeRight;
	[SerializeField] CommandItem m_SwipeUp;
	[SerializeField] CommandItem m_SwipeDown;
	[SerializeField] CommandItem m_Tap;

	[SerializeField] float m_DragDistance;

	Vector2 m_Position;

	readonly Queue<CommandItem> m_Commands = new Queue<CommandItem>();

	public void Show(float _Duration, CommandType _CommandType)
	{
		CommandItem commandItem;
		switch (_CommandType)
		{
			case CommandType.SwipeLeft:
				commandItem = Instantiate(m_SwipeLeft, transform);
				break;
			case CommandType.SwipeRight:
				commandItem = Instantiate(m_SwipeRight, transform);
				break;
			case CommandType.SwipeUp:
				commandItem = Instantiate(m_SwipeUp, transform);
				break;
			case CommandType.SwipeDown:
				commandItem = Instantiate(m_SwipeDown, transform);
				break;
			case CommandType.Tap:
				commandItem = Instantiate(m_Tap, transform);
				break;
			default:
				throw new Exception();
		}
		
		commandItem.Show(_Duration);
		
		m_Commands.Enqueue(commandItem);
		
		Invoke(nameof(Hide), _Duration + 0.2f);
	}

	public void Hide()
	{
		CommandItem commandItem = m_Commands.Dequeue();
		
		commandItem.Fail(0.2f);
		commandItem.Hide(0.2f);
	}

	void ProcessCommand(CommandType _CommandType)
	{
		if (m_Commands.Count == 0)
			return;
		
		CommandItem commandItem = m_Commands.Peek();
		
		if (commandItem.Type == _CommandType)
			commandItem.Success(0.2f);
		else
			commandItem.Fail(0.2f);
	}

	public void OnDrag(PointerEventData _EventData)
	{
		if (m_Commands.Count == 0)
			return;
		
		CommandItem commandItem = m_Commands.Peek();
		
		switch (commandItem.Type)
		{
			case CommandType.SwipeLeft:
				commandItem.transform.localPosition += new Vector3(Mathf.Min(0, _EventData.delta.x), 0, 0);
				break;
			
			case CommandType.SwipeRight:
				commandItem.transform.localPosition += new Vector3(Mathf.Max(0, _EventData.delta.x), 0, 0);
				break;
			
			case CommandType.SwipeUp:
				commandItem.transform.localPosition += new Vector3(0, _EventData.delta.y, 0);
				break;
			
			case CommandType.SwipeDown:
				commandItem.transform.localPosition -= new Vector3(0, _EventData.delta.y, 0);
				break;
		}
	}

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	public void OnPointerDown(PointerEventData _EventData)
	{
		m_Position = _EventData.position;
	}

	public void OnPointerUp(PointerEventData _EventData)
	{
		Vector2 delta = _EventData.position - m_Position;
		
		CommandType commandType;
		if (Mathf.Abs(delta.x) > m_DragDistance || Mathf.Abs(delta.y) > m_DragDistance)
		{
			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
				commandType = delta.x < 0 ? CommandType.SwipeLeft : CommandType.SwipeRight;
			else
				commandType = delta.y < 0 ? CommandType.SwipeDown : CommandType.SwipeUp;
		}
		else
		{
			commandType = CommandType.Tap;
		}
		
		ProcessCommand(commandType);
	}
}
