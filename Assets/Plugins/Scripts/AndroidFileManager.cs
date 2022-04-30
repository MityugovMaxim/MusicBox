#if UNITY_ANDROID
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public class AndroidFileManager : IFileManager
{
	public class SelectSuccessHandler : AndroidJavaProxy
	{
		readonly Action<string> m_Action;

		public SelectSuccessHandler(Action<string> _Action) : base(SUCCESS_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		void Invoke(string _Path)
		{
			m_Action?.Invoke(_Path);
		}
	}

	public class SelectCancelHandler : AndroidJavaProxy
	{
		readonly Action m_Action;

		public SelectCancelHandler(Action _Action) : base(CANCEL_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		void Invoke()
		{
			m_Action?.Invoke();
		}
	}

	public class SelectFailHandler : AndroidJavaProxy
	{
		readonly Action<string> m_Action;

		public SelectFailHandler(Action<string> _Action) : base(FAIL_NAME)
		{
			m_Action = _Action;
		}

		[Preserve]
		void Invoke(string _Error)
		{
			m_Action?.Invoke(_Error);
		}
	}

	const string CLASS_NAME   = "com.audiobox.filecontroller.FileController";
	const string SUCCESS_NAME = "com.audiobox.filecontroller.SelectSuccessHandler";
	const string CANCEL_NAME  = "com.audiobox.filecontroller.SelectCancelHandler";
	const string FAIL_NAME    = "com.audiobox.filecontroller.SelectFailHandler";

	Action<string> m_Success;
	Action         m_Cancel;
	Action<string> m_Fail;

	Task<string> IFileManager.SelectFile(string _Extension, CancellationToken _Token)
	{
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();

		if (_Token.IsCancellationRequested)
		{
			completionSource.SetCanceled();
			return completionSource.Task;
		}
		
		m_Success = _Path => completionSource.SetResult(_Path);
		m_Cancel  = () => completionSource.SetCanceled();
		m_Fail    = _Error => completionSource.SetException(new Exception(_Error));
		
		_Token.Register(m_Cancel);
		
		using (AndroidJavaClass controller = new AndroidJavaClass(CLASS_NAME))
		{
			controller.CallStatic(
				"SelectFile",
				_Extension,
				new SelectSuccessHandler(m_Success),
				new SelectCancelHandler(m_Cancel),
				new SelectFailHandler(m_Fail)
			);
		}
		
		return completionSource.Task;
	}
}
#endif