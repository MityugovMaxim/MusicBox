using System.Collections.Generic;
using System.Threading.Tasks;
using AudioBox.Logging;
using Zenject;

[Menu(MenuType.LoginMenu)]
public class UILoginMenu : UIMenu
{
	const int LOGIN_ATTEMPT_LIMIT = 2;

	[Inject] SocialProcessor    m_SocialProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;
	[Inject] MenuProcessor      m_MenuProcessor;

	[Inject] IDataCollection[] m_Collections;
	[Inject] IDataObject[]     m_Objects;

	public async Task Login()
	{
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Launch);
		
		int attempt = 0;
		
		while (true)
		{
			bool login = await m_SocialProcessor.Login();
			
			if (login)
				break;
			
			await Task.Delay(150);
			
			attempt++;
			
			if (attempt < LOGIN_ATTEMPT_LIMIT)
				continue;
			
			TaskCompletionSource<bool> retry = new TaskCompletionSource<bool>();
			
			await m_MenuProcessor.RetryAsync(
				"login",
				() => retry.TrySetResult(true)
			);
			
			await retry.Task;
			
			await Task.Delay(250);
			
			attempt = 0;
		}
		
		m_StatisticProcessor.LogTechnicalStep(TechnicalStepType.Login);
		
		m_StatisticProcessor.LogLogin(m_SocialProcessor.UserID, m_SocialProcessor.Name);
		
		Log.Info(this, "Login complete. User ID: {0}.", m_SocialProcessor.UserID);
		
		await LoadObjects();
		
		await LoadCollections(DataPriority.High);
		
		await LoadCollections(DataPriority.Medium);
		
		await m_MenuProcessor.Show(MenuType.MainMenu, true);
		
		await m_MenuProcessor.Hide(MenuType.LoginMenu);
		
		await LoadCollections(DataPriority.Low);
	}

	Task LoadCollections(DataPriority _Priority)
	{
		List<Task> loading = new List<Task>();
		foreach (IDataCollection data in m_Collections)
		{
			if (data != null && data.Priority == _Priority)
				loading.Add(data.Reload());
		}
		return Task.WhenAll(loading);
	}

	async Task LoadObjects()
	{
		foreach (IDataObject data in m_Objects)
		{
			if (data != null)
				await data.Reload();
		}
	}
}
