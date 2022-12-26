using System;
using System.Threading;
using System.Threading.Tasks;

public static class TaskExtension
{
	public static Task Dispatch(this Task _Task, Action<Task> _Action, CancellationToken _Token = default)
	{
		return _Task.ContinueWith(_T => UnityTask.Dispatch(() => _Action?.Invoke(_T)), _Token);
	}
}
