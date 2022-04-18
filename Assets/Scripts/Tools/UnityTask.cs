using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityTask : MonoBehaviour
{
	static          UnityTask                         m_Instance;
	static readonly Queue<TaskCompletionSource<bool>> m_YieldTasks = new Queue<TaskCompletionSource<bool>>();

	void Awake()
	{
		m_Instance = this;
	}

	void OnDestroy()
	{
		m_Instance = null;
	}

	void OnEnable()
	{
		StartCoroutine(YieldRoutine());
	}

	public static Task Yield(CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		_Token.Register(() => completionSource.TrySetCanceled());
		
		m_YieldTasks.Enqueue(completionSource);
		
		return completionSource.Task;
	}

	public static Task Tick(Action _Action, int _Frequency, float _Duration, CancellationToken _Token = default)
	{
		return Tick(_Action, _Frequency, 0, _Duration, _Token);
	}

	public static Task Tick(Action _Action, int _Frequency, float _Delay, float _Duration, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = TickRoutine(_Action, _Frequency, _Delay, _Duration, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task Lerp(
		Action<float>     _Action,
		float             _Source,
		float             _Target,
		float             _Duration,
		CancellationToken _Token = default
	)
	{
		return Lerp(_Action, _Source, _Target, 0, _Duration, AnimationCurve.Linear(0, 0, 1, 1), _Token);
	}

	public static Task Lerp(
		Action<float>     _Action,
		float             _Source,
		float             _Target,
		float             _Duration,
		AnimationCurve    _Curve,
		CancellationToken _Token = default
	)
	{
		return Lerp(_Action, _Source, _Target, 0, _Duration, _Curve, _Token);
	}

	public static Task Lerp(
		Action<float>     _Action,
		float             _Source,
		float             _Target,
		float             _Delay,
		float             _Duration,
		AnimationCurve    _Curve,
		CancellationToken _Token = default
	)
	{
		if (Mathf.Approximately(_Source, _Target))
		{
			_Delay    = 0;
			_Duration = 0;
		}
		
		return Phase(
			_Phase =>
			{
				float phase = _Curve.Evaluate(_Phase);
				float value = Mathf.Lerp(_Source, _Target, phase);
				_Action(value);
			},
			_Delay,
			_Duration,
			_Token
		);
	}

	public static Task Phase(Action<float> _Action, float _Duration, CancellationToken _Token = default)
	{
		return Phase(_Action, 0, _Duration, _Token);
	}

	public static Task Phase(Action<float> _Action, float _Delay, float _Duration, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = PhaseRoutine(_Action, _Delay, _Duration, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task While(Func<bool> _Condition, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = WhileRoutine(_Condition, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task Until(Func<bool> _Condition, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = UntilRoutine(_Condition, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task Delay(float _Delay, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = DelayRoutine(_Delay, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task Instruction(YieldInstruction _Instruction, CancellationToken _Token = default)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		IEnumerator routine = InstructionRoutine(_Instruction, () => completionSource.SetResult(true));
		
		_Token.Register(
			() =>
			{
				if (routine != null && !ReferenceEquals(m_Instance, null))
					m_Instance.StopCoroutine(routine);
				
				completionSource.TrySetCanceled();
			}
		);
		
		m_Instance.StartCoroutine(routine);
		
		return completionSource.Task;
	}

	public static Task UnloadAssets(CancellationToken _Token)
	{
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		AsyncOperation operation = Resources.UnloadUnusedAssets();
		
		operation.completed += _Result =>
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			
			completionSource.SetResult(true);
		};
		
		return completionSource.Task;
	}

	static IEnumerator YieldRoutine()
	{
		while (true)
		{
			yield return null;
			
			while (m_YieldTasks.Count > 0)
				m_YieldTasks.Dequeue().SetResult(true);
		}
	}

	static IEnumerator TickRoutine(Action _Action, int _Frequency, float _Delay, float _Duration, Action _Finished)
	{
		if (_Action == null)
		{
			_Finished?.Invoke();
			yield break;
		}
		
		if (_Delay > float.Epsilon)
			yield return new WaitForSeconds(_Delay);
		
		if (_Duration > float.Epsilon)
		{
			float time      = 0;
			float step      = 1.0f / _Frequency;
			float threshold = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				if (time < threshold)
					continue;
				
				threshold += step;
				
				_Action();
			}
		}
		
		_Finished?.Invoke();
	}

	static IEnumerator PhaseRoutine(Action<float> _Action, float _Delay, float _Duration, Action _Finished)
	{
		if (_Action == null)
		{
			_Finished?.Invoke();
			yield break;
		}
		
		if (_Delay > float.Epsilon)
			yield return new WaitForSeconds(_Delay);
		
		_Action(0);
		
		if (_Duration > float.Epsilon)
		{
			float time = 0;
			while (time < _Duration)
			{
				yield return null;
				
				time += Time.deltaTime;
				
				_Action(time / _Duration);
			}
		}
		
		_Action(1);
		
		_Finished?.Invoke();
	}

	static IEnumerator WhileRoutine(Func<bool> _Condition, Action _Finished)
	{
		if (_Condition != null)
			yield return new WaitWhile(_Condition);
		
		_Finished?.Invoke();
	}

	static IEnumerator UntilRoutine(Func<bool> _Condition, Action _Finished)
	{
		if (_Condition != null)
			yield return new WaitUntil(_Condition);
		
		_Finished?.Invoke();
	}

	static IEnumerator DelayRoutine(float _Delay, Action _Finished)
	{
		if (_Delay > float.Epsilon)
			yield return new WaitForSeconds(_Delay);
		
		_Finished?.Invoke();
	}

	static IEnumerator InstructionRoutine(YieldInstruction _Instruction, Action _Finished)
	{
		if (_Instruction != null)
			yield return _Instruction;
		
		_Finished?.Invoke();
	}
}