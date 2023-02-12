using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskProvider
{
	static readonly HashSet<string>                                m_Groups = new HashSet<string>();
	static readonly Dictionary<string, TaskCompletionSource<bool>> m_Tasks  = new Dictionary<string, TaskCompletionSource<bool>>();

	public static void Restore()
	{
		foreach (TaskCompletionSource<bool> task in m_Tasks.Values)
			task.TrySetCanceled();
		
		m_Groups.Clear();
		m_Tasks.Clear();
	}

	public static Func<Task>[] Group(params Func<Task>[] _Tasks) => _Tasks;

	public static Task<bool> ProcessAsync<T>(params Func<Task>[] _Preprocess)
	{
		string group = typeof(T).Name;
		
		return ProcessAsync(group, _Preprocess, null);
	}

	public static Task<bool> ProcessAsync<T>(Func<Task>[] _Preprocess, Func<Task>[] _Postprocess)
	{
		string group = typeof(T).Name;
		
		return ProcessAsync(group, _Preprocess, _Postprocess);
	}

	public static Task<bool> ProcessAsync<T>(T _Context, params Func<Task>[] _Preprocess)
	{
		string group = _Context.GetType().Name;
		
		return ProcessAsync(group, _Preprocess, null);
	}

	public static Task<bool> ProcessAsync<T>(T _Context, Func<Task>[] _Preprocess, Func<Task>[] _Postprocess)
	{
		string group = _Context.GetType().Name;
		
		return ProcessAsync(group, _Preprocess, _Postprocess);
	}

	public static async Task<bool> ProcessAsync(string _Group, Func<Task>[] _Preprocess, Func<Task>[] _Postprocess)
	{
		if (m_Groups.Contains(_Group))
			return true;
		
		if (m_Tasks.TryGetValue(_Group, out TaskCompletionSource<bool> task))
			return await task.Task;
		
		task = new TaskCompletionSource<bool>();
		
		m_Tasks[_Group] = task;
		
		int frame = Time.frameCount;
		
		IEnumerable<Task> preprocess = _Preprocess?
			.Where(_Task => _Task != null)
			.Select(_Task => _Task.Invoke())
			.Where(_Task => _Task != null);
		
		if (preprocess != null)
			await Task.WhenAll(preprocess);
		
		IEnumerable<Task> postprocess = _Postprocess?
			.Where(_Task => _Task != null)
			.Select(_Task => _Task.Invoke())
			.Where(_Task => _Task != null);
		
		if (postprocess != null)
			await Task.WhenAll(postprocess);
		
		m_Groups.Add(_Group);
		
		m_Tasks[_Group] = task;
		
		if (m_Tasks.ContainsKey(_Group))
			m_Tasks.Remove(_Group);
		
		bool instant = frame == Time.frameCount;
		
		task.TrySetResult(instant);
		
		return instant;
	}
}
