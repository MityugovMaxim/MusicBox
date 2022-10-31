using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Menu(MenuType.PermissionMenu)]
public class UIPermissionMenu : UIAnimationMenu
{
	enum State
	{
		None    = 0,
		Allow   = 1,
		Decline = 2,
	}

	const string PERMISSION_KEY = "PERMISSION_{0}";

	[SerializeField] TMP_Text m_Title;
	[SerializeField] TMP_Text m_Description;
	[SerializeField] Button   m_AllowButton;
	[SerializeField] Button   m_CancelButton;

	string m_Permission;

	Action<bool> m_Finished;

	protected override void Awake()
	{
		base.Awake();
		
		m_AllowButton.Subscribe(Allow);
		m_CancelButton.Subscribe(Cancel);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_AllowButton.Unsubscribe(Allow);
		m_CancelButton.Unsubscribe(Cancel);
	}

	public void Setup(string _Title, string _Description)
	{
		m_Title.text       = _Title;
		m_Description.text = _Description;
	}

	public Task<bool> Process(string _Permission)
	{
		m_Permission = _Permission;
		
		InvokeFinished(false);
		
		State state = LoadPermission(_Permission);
		
		if (state != State.None)
			return Task.FromResult(false);
		
		Show();
		
		TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
		
		m_Finished = _State => completionSource.TrySetResult(_State);
		
		return completionSource.Task;
	}

	async void Allow()
	{
		SavePermission(m_Permission, State.Allow);
		
		await HideAsync();
		
		InvokeFinished(true);
	}

	async void Cancel()
	{
		SavePermission(m_Permission, State.Decline);
		
		await HideAsync();
		
		InvokeFinished(false);
	}

	void SavePermission(string _Permission, State _State)
	{
		if (string.IsNullOrEmpty(_Permission))
			return;
		
		string key = string.Format(PERMISSION_KEY, _Permission);
		
		PlayerPrefs.SetInt(key, (int)_State);
	}

	State LoadPermission(string _Permission)
	{
		if (string.IsNullOrEmpty(_Permission))
			return State.None;
		
		string key = string.Format(PERMISSION_KEY, _Permission);
		
		return (State)PlayerPrefs.GetInt(key, 0);
	}

	void InvokeFinished(bool _State)
	{
		Action<bool> action = m_Finished;
		m_Finished = null;
		action?.Invoke(_State);
	}
}
