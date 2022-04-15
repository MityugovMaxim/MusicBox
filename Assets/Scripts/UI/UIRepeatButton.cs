using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Zenject;

public class UIRepeatButton : UIEntity, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Serializable]
	public class Repeat
	{
		public int Count => m_Count;
		public int Delay => m_Delay;

		[SerializeField] int m_Count;
		[SerializeField] int m_Delay;
	}

	Animator m_Animator;

	static readonly int m_NormalParameterID  = Animator.StringToHash("Normal");
	static readonly int m_PressedParameterID = Animator.StringToHash("Pressed");

	[SerializeField] Repeat[] m_Repeats;

	[Header("Sound")]
	[SerializeField, Sound] string m_DownSound;
	[SerializeField, Sound] string m_UpSound;
	[SerializeField, Sound] string m_RepeatSound;

	[Header("Haptic")]
	[SerializeField] Haptic.Type m_DownHaptic;
	[SerializeField] Haptic.Type m_UpHaptic;
	[SerializeField] Haptic.Type m_RepeatHaptic;

	[SerializeField] UnityEvent m_Action;

	[Inject] HapticProcessor m_HapticProcessor;
	[Inject] SoundProcessor  m_SoundProcessor;

	bool m_Pressed;
	bool m_Hovered;

	CancellationTokenSource m_TokenSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_Animator = GetComponent<Animator>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		StopRepeat();
	}

	void StopRepeat()
	{
		m_TokenSource?.Cancel();
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	async void StartRepeat()
	{
		StopRepeat();
		
		Repeat[] repeats = m_Repeats.OrderBy(_Repeat => _Repeat.Count).ToArray();
		
		if (repeats.Length == 0)
		{
			m_Action?.Invoke();
			return;
		}
		
		m_TokenSource = new CancellationTokenSource();
		
		CancellationToken token = m_TokenSource.Token;
		
		try
		{
			int count = 0;
			int index = 0;
			while (!token.IsCancellationRequested && m_Hovered && m_Pressed)
			{
				Repeat repeat = repeats[index];
				
				m_HapticProcessor.Process(m_RepeatHaptic);
				
				m_SoundProcessor.Play(m_RepeatSound);
				
				m_Action?.Invoke();
				
				await Task.Delay(repeat.Delay, token);
				
				count++;
				
				if (count > repeat.Count)
					index = Mathf.Min(index + 1, repeats.Length - 1);
			}
		}
		catch (TaskCanceledException) { }
		
		if (token.IsCancellationRequested)
			return;
		
		m_TokenSource?.Dispose();
		m_TokenSource = null;
	}

	void NormalState()
	{
		m_Animator.ResetTrigger(m_PressedParameterID);
		m_Animator.SetTrigger(m_NormalParameterID);
	}

	void PressedState()
	{
		m_Animator.ResetTrigger(m_NormalParameterID);
		m_Animator.SetTrigger(m_PressedParameterID);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData _EventData)
	{
		m_Pressed = true;
		m_Hovered = true;
		
		m_HapticProcessor.Process(m_DownHaptic);
		m_SoundProcessor.Play(m_DownSound);
		
		PressedState();
		
		StartRepeat();
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData _EventData)
	{
		m_Pressed = false;
		m_Hovered = false;
		
		m_HapticProcessor.Process(m_UpHaptic);
		m_SoundProcessor.Play(m_UpSound);
		
		NormalState();
		
		StopRepeat();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData _EventData)
	{
		if (!m_Pressed || m_Hovered)
			return;
		
		m_Hovered = true;
		
		PressedState();
		
		StartRepeat();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData _EventData)
	{
		if (!m_Pressed || !m_Hovered)
			return;
		
		m_Hovered = false;
		
		NormalState();
		
		StopRepeat();
	}
}