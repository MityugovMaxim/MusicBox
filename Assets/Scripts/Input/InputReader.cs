using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InputType
{
	TouchDown  = 0,
	TouchUp    = 1,
	SwipeLeft  = 2,
	SwipeRight = 3,
	SwipeUp    = 4,
	SwipeDown  = 5,
}

public abstract class InputIndicatorView : UIBehaviour
{
	protected RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	RectTransform m_RectTransform;

	public abstract void Process(float _Time);

	public abstract void Success();

	public abstract void Fail();
}

public abstract class InputZoneView : UIBehaviour
{
	protected RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	RectTransform m_RectTransform;

	public abstract void Setup(float _Time, float _MinOffset, float _MaxOffset);
}

public class InputReader : MonoBehaviour
{
	[SerializeField] InputType          m_InputType;
	[SerializeField] InputZoneView      m_InputZone;
	[SerializeField] InputIndicatorView m_InputIndicator;

	readonly Dictionary<int, InputIndicatorView> m_InputIndicators = new Dictionary<int, InputIndicatorView>();
	readonly Queue<int>                          m_InputIDs        = new Queue<int>();
	int                                          m_InputID;

	public void SetupZone(float _Time, float _MinOffset, float _MaxOffset)
	{
		if (m_InputZone != null)
			m_InputZone.Setup(_Time, _MinOffset, _MaxOffset);
	}

	public void StartProcessing(int _ID, float _Time)
	{
		CreateInputIndicator(_ID);
		
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		inputIndicator.Process(_Time);
	}

	public void UpdateProcessing(int _ID, float _Time)
	{
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		inputIndicator.Process(_Time);
	}

	public void FinishProcessing(int _ID, float _Time)
	{
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		inputIndicator.Process(_Time);
		
		RemoveInputIndicator(_ID);
	}

	public void RestoreProcessing(int _ID, float _Time)
	{
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		inputIndicator.Process(_Time);
		
		RemoveInputIndicator(_ID);
	}

	public void StartInput(int _ID)
	{
		m_InputIDs.Enqueue(_ID);
	}

	public void FinishInput(int _ID)
	{
		int inputID = m_InputIDs.Peek();
		
		if (inputID != _ID)
			return;
		
		m_InputIDs.Dequeue();
		
		InputIndicatorView inputIndicatorView = GetInputView(_ID);
		
		inputIndicatorView.Fail();
	}

	public void ProcessInput(InputType _InputType)
	{
		if (m_InputIDs.Count == 0)
			return;
		
		int inputID = m_InputIDs.Peek();
		
		if (_InputType == InputType.TouchDown)
			m_InputID = inputID;
		
		if (m_InputID != inputID)
			return;
		
		if (_InputType == InputType.TouchUp)
		{
			m_InputIDs.Dequeue();
			return;
		}
		
		InputIndicatorView inputIndicatorView = GetInputView(m_InputID);
		
		if (m_InputType == _InputType)
			inputIndicatorView.Success();
		else if (_InputType != InputType.TouchDown)
			inputIndicatorView.Fail();
	}

	void CreateInputIndicator(int _ID)
	{
		if (m_InputIndicator == null)
		{
			Debug.LogError($"[{GetType().Name}] Create input indicator failed. Indicator prefab is null.", gameObject);
			return;
		}
		
		m_InputIndicators[_ID] = Instantiate(m_InputIndicator, transform);
	}

	void RemoveInputIndicator(int _ID)
	{
		if (!m_InputIndicators.ContainsKey(_ID))
		{
			Debug.LogError($"[{GetType().Name}] Remove input indicator failed. Indicator with id '{_ID}' doesn't exists.", gameObject);
			return;
		}
		
		InputIndicatorView inputIndicator = m_InputIndicators[_ID];
		
		m_InputIndicators.Remove(_ID);
		
		if (inputIndicator == null)
		{
			Debug.LogError($"[{GetType().Name}] Remove input indicator failed. Indicator with id '{_ID}' is null.", gameObject);
			return;
		}
		
		Destroy(inputIndicator.gameObject);
	}

	InputIndicatorView GetInputView(int _ID)
	{
		if (m_InputIndicators.ContainsKey(_ID))
			return m_InputIndicators[_ID];
		
		Debug.LogError($"[{GetType().Name}] Get input indicator failed. Indicator with id '{_ID}' doesn't exists.", gameObject);
		
		return null;
	}
}