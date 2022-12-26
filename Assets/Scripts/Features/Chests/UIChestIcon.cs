using System;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
public class UIChestIcon : UIEntity
{
	const string PROGRESS_STATE = "progress";
	const string COLLECT_STATE  = "collect";
	const string SELECT_STATE   = "select";
	const string READY_STATE    = "ready";
	const string OPEN_STATE     = "open";

	static readonly int m_ProgressParameterID = Animator.StringToHash("Progress");
	static readonly int m_CollectParameterID  = Animator.StringToHash("Collect");
	static readonly int m_SelectParameterID   = Animator.StringToHash("Select");
	static readonly int m_ProcessParameterID  = Animator.StringToHash("Process");
	static readonly int m_ReadyParameterID    = Animator.StringToHash("Ready");
	static readonly int m_OpenParameterID     = Animator.StringToHash("Open");
	static readonly int m_RestoreParameterID  = Animator.StringToHash("Restore");

	static readonly int m_ProcessStateID = Animator.StringToHash("Process");
	static readonly int m_ReadyStateID   = Animator.StringToHash("Ready");

	public RankType ChestRank
	{
		get => m_ChestRank;
		set
		{
			if (m_ChestRank == value)
				return;
			
			m_ChestRank = value;
			
			ProcessRank();
		}
	}

	Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	[SerializeField] RankType  m_ChestRank;
	[SerializeField] Renderer  m_Renderer;
	[SerializeField] Material  m_Bronze;
	[SerializeField] Material  m_Silver;
	[SerializeField] Material  m_Gold;
	[SerializeField] Material  m_Platinum;
	[SerializeField] Transform m_Object;
	[SerializeField] Transform m_Horizontal;
	[SerializeField] Transform m_Vertical;
	[SerializeField] Vector2   m_Depth = new Vector2(10, 15);

	Animator m_Animator;
	Action   m_ProgressFinished;
	Action   m_CollectFinished;
	Action   m_SelectFinished;
	Action   m_ReadyFinished;
	Action   m_OpenFinished;
	Vector3  m_Position;

	protected override void Awake()
	{
		base.Awake();
		
		Animator.SubscribeComplete(PROGRESS_STATE, InvokeProgressFinished);
		Animator.SubscribeComplete(COLLECT_STATE, InvokeCollectFinished);
		Animator.SubscribeComplete(SELECT_STATE, InvokeSelectFinished);
		Animator.SubscribeComplete(READY_STATE, InvokeReadyFinished);
		Animator.SubscribeComplete(OPEN_STATE, InvokeOpenFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		Animator.UnsubscribeComplete(PROGRESS_STATE, InvokeProgressFinished);
		Animator.UnsubscribeComplete(COLLECT_STATE, InvokeCollectFinished);
		Animator.UnsubscribeComplete(SELECT_STATE, InvokeSelectFinished);
		Animator.UnsubscribeComplete(READY_STATE, InvokeReadyFinished);
		Animator.UnsubscribeComplete(OPEN_STATE, InvokeOpenFinished);
	}

	void Update()
	{
		if (m_Position != m_Object.position)
			ProcessRotation();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		ProcessRank();
		
		ProcessRotation();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		Restore();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (!IsInstanced || Application.isPlaying)
			return;
		
		ProcessRank();
		
		ProcessRotation();
	}
	#endif

	public async void Progress() => await ProgressAsync();

	public async void Collect() => await CollectAsync();

	public async void Select() => await SelectAsync();

	public void Process()
	{
		if (Animator.CheckState(m_ProcessStateID))
			return;
		
		Animator.SetTrigger(m_ProcessParameterID);
	}

	public async void Ready() => await ReadyAsync();

	public async void Open() => await OpenAsync();

	public Task ProgressAsync()
	{
		InvokeProgressFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_ProgressFinished = () => source.TrySetResult(true);
		
		Animator.SetTrigger(m_ProgressParameterID);
		
		return source.Task;
	}

	public Task CollectAsync()
	{
		InvokeCollectFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_CollectFinished = () => source.TrySetResult(true);
		
		Animator.SetTrigger(m_CollectParameterID);
		
		return source.Task;
	}

	public Task SelectAsync()
	{
		InvokeSelectFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_SelectFinished = () => source.TrySetResult(true);
		
		Animator.SetTrigger(m_SelectParameterID);
		
		return source.Task;
	}

	public Task ReadyAsync()
	{
		if (Animator.CheckState(m_ReadyStateID))
			return Task.CompletedTask;
		
		InvokeReadyFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_ReadyFinished = () => source.TrySetResult(true);
		
		Animator.SetTrigger(m_ReadyParameterID);
		
		return source.Task;
	}

	public Task OpenAsync()
	{
		InvokeOpenFinished();
		
		TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
		
		m_OpenFinished = () => source.TrySetResult(true);
		
		Animator.SetTrigger(m_OpenParameterID);
		
		return source.Task;
	}

	public void Restore()
	{
		InvokeProgressFinished();
		InvokeCollectFinished();
		InvokeSelectFinished();
		InvokeReadyFinished();
		InvokeOpenFinished();
		
		Animator.ResetTrigger(m_ProgressParameterID);
		Animator.ResetTrigger(m_CollectParameterID);
		Animator.ResetTrigger(m_SelectParameterID);
		Animator.ResetTrigger(m_ProcessParameterID);
		Animator.ResetTrigger(m_ReadyParameterID);
		Animator.ResetTrigger(m_OpenParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
		Animator.Update(0);
	}

	void ProcessRank()
	{
		switch (m_ChestRank)
		{
			case RankType.Bronze:
				m_Renderer.enabled  = true;
				m_Renderer.material = m_Bronze;
				break;
			case RankType.Silver:
				m_Renderer.enabled  = true;
				m_Renderer.material = m_Silver;
				break;
			case RankType.Gold:
				m_Renderer.enabled  = true;
				m_Renderer.material = m_Gold;
				break;
			case RankType.Platinum:
				m_Renderer.enabled  = true;
				m_Renderer.material = m_Platinum;
				break;
			default:
				m_Renderer.enabled  = false;
				m_Renderer.material = null;
				break;
		}
	}

	void ProcessRotation()
	{
		m_Position = m_Object.position;
		
		Vector2 offset = RectTransformUtility.WorldToScreenPoint(
			null,
			m_Position
		);
		
		Vector2 amplitude = new Vector2(
			Mathf.Lerp(m_Depth.x, -m_Depth.x,offset.x / Screen.width),
			Mathf.Lerp(-m_Depth.y, m_Depth.y,offset.y / Screen.height)
		);
		
		m_Horizontal.localEulerAngles = new Vector3(0, amplitude.x, 0);
		m_Vertical.localEulerAngles   = new Vector3(amplitude.y, 0, 0);
	}

	void InvokeProgressFinished()
	{
		Action action = m_ProgressFinished;
		m_ProgressFinished = null;
		action?.Invoke();
	}

	void InvokeCollectFinished()
	{
		Action action = m_CollectFinished;
		m_CollectFinished = null;
		action?.Invoke();
	}

	void InvokeSelectFinished()
	{
		Action action = m_SelectFinished;
		m_SelectFinished = null;
		action?.Invoke();
	}

	void InvokeReadyFinished()
	{
		Action action = m_ReadyFinished;
		m_ReadyFinished = null;
		action?.Invoke();
	}

	void InvokeOpenFinished()
	{
		Action action = m_OpenFinished;
		m_OpenFinished = null;
		action?.Invoke();
	}
}
