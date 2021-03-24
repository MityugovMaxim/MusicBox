using System.Collections.Generic;
using UnityEngine;

public enum InputType
{
	None       = -1,
	TouchDown  = 0,
	TouchUp    = 1,
	SwipeLeft  = 2,
	SwipeRight = 3,
	SwipeUp    = 4,
	SwipeDown  = 5,
}

public class InputReader : MonoBehaviour
{
	[SerializeField] InputType          m_InputType;
	[SerializeField] InputZoneView      m_InputZone;
	[SerializeField] InputIndicatorView m_InputIndicator;
	[SerializeField] InputFXView        m_InputFX;
	[SerializeField] InputEvent         m_OnSuccess;
	[SerializeField] InputEvent         m_OnFail;

	readonly Pool<InputIndicatorView> m_IndicatorPool = new Pool<InputIndicatorView>();
	readonly Pool<InputFXView>        m_FXPool        = new Pool<InputFXView>();

	readonly Dictionary<int, InputIndicatorView> m_InputIndicators = new Dictionary<int, InputIndicatorView>();
	readonly Queue<int>                          m_InputIDs        = new Queue<int>();
	int                                          m_InputID;

	void OnDestroy()
	{
		m_IndicatorPool?.Release();
		m_FXPool?.Release();
	}

	public void SetupZone(float _Zone, float _ZoneMin, float _ZoneMax)
	{
		if (m_InputZone != null)
			m_InputZone.Setup(_Zone, _ZoneMin, _ZoneMax);
	}

	public void StartProcessing(int _ID, float _Time)
	{
		CreateInputIndicator(_ID);
		
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		if (inputIndicator != null)
		{
			inputIndicator.Begin();
			inputIndicator.Process(_Time);
		}
	}

	public void UpdateProcessing(int _ID, float _Time)
	{
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		if (inputIndicator != null)
			inputIndicator.Process(_Time);
	}

	public void FinishProcessing(int _ID, float _Time)
	{
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		if (inputIndicator != null)
		{
			inputIndicator.Process(_Time);
			inputIndicator.Complete(() => RemoveInputIndicator(_ID));
		}
	}

	public void StartInput(int _ID)
	{
		m_InputIDs.Enqueue(_ID);
	}

	public void FinishInput(int _ID)
	{
		if (m_InputIDs.Count == 0)
			return;
		
		int inputID = m_InputIDs.Peek();
		
		if (inputID != _ID)
			return;
		
		m_InputIDs.Dequeue();
		
		InputIndicatorView inputIndicator = GetInputView(_ID);
		
		if (inputIndicator != null && Application.isPlaying)
			inputIndicator.Fail(() => RemoveInputIndicator(_ID));
	}

	public void ProcessInput(InputType _InputType)
	{
		if (m_InputIDs.Count == 0)
			return;
		
		int inputID = m_InputIDs.Peek();
		
		if (_InputType != InputType.TouchUp)
			m_InputID = inputID;
		
		if (m_InputID != inputID)
			return;
		
		if (_InputType == InputType.TouchUp)
		{
			m_InputIDs.Dequeue();
			return;
		}
		
		InputIndicatorView inputIndicator = GetInputView(m_InputID);
		
		if (m_InputType == _InputType)
		{
			inputIndicator.Success(() => RemoveInputIndicator(m_InputID));
			m_InputIDs.Dequeue();
			
			PlayInputFX();
		}
		else if (_InputType != InputType.TouchDown)
		{
			inputIndicator.Fail(() => RemoveInputIndicator(m_InputID));
			m_InputIDs.Dequeue();
		}
	}

	void CreateInputIndicator(int _ID)
	{
		if (m_InputIndicator == null)
		{
			Debug.LogError($"[{GetType().Name}] Create input indicator failed. Indicator prefab is null.", gameObject);
			return;
		}
		
		InputIndicatorView indicatorView = m_IndicatorPool.Instantiate(m_InputIndicator, transform);
		
		m_InputIndicators[_ID] = indicatorView;
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
		
		m_IndicatorPool.Remove(inputIndicator);
	}

	void PlayInputFX()
	{
		InputFXView inputFX = m_FXPool.Instantiate(m_InputFX, transform);
		
		if (inputFX != null)
			inputFX.Play(() => m_FXPool.Remove(inputFX));
	}

	InputIndicatorView GetInputView(int _ID)
	{
		return m_InputIndicators.ContainsKey(_ID) ? m_InputIndicators[_ID] : null;
	}

	void InvokeSuccess()
	{
		m_OnSuccess?.Invoke(m_InputType);
	}

	void InvokeFail()
	{
		m_OnFail?.Invoke(m_InputType);
	}
}