using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ConsentMenu)]
public class UIConsentMenu : UIAnimationMenu
{
	const string CONSENT_PROCESSED_KEY = "CONSENT_PROCESSED";

	public static bool Processed
	{
		get => PlayerPrefs.GetInt(CONSENT_PROCESSED_KEY, 0) > 0;
		private set => PlayerPrefs.SetInt(CONSENT_PROCESSED_KEY, value ? 1 : 0);
	}

	[SerializeField] Button m_TermsOfServiceButton;
	[SerializeField] Button m_PrivacyPolicyButton;
	[SerializeField] Button m_AcceptButton;

	[SerializeField] string m_TermsOfServiceURL;
	[SerializeField] string m_PrivacyPolicyURL;

	[Inject] MenuProcessor m_MenuProcessor;

	TaskCompletionSource<bool> m_CompletionSource;

	protected override void Awake()
	{
		base.Awake();
		
		m_TermsOfServiceButton.onClick.AddListener(TermsOfService);
		m_PrivacyPolicyButton.onClick.AddListener(PrivacyPolicy);
		m_AcceptButton.onClick.AddListener(Accept);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_TermsOfServiceButton.onClick.RemoveListener(TermsOfService);
		m_PrivacyPolicyButton.onClick.RemoveListener(PrivacyPolicy);
		m_AcceptButton.onClick.RemoveListener(Accept);
	}

	public Task ProcessAsync()
	{
		if (m_CompletionSource != null)
			return m_CompletionSource.Task;
		
		if (Processed)
			return Task.CompletedTask;
		
		m_CompletionSource = new TaskCompletionSource<bool>();
		
		Show();
		
		return m_CompletionSource.Task;
	}

	void TermsOfService()
	{
		if (string.IsNullOrEmpty(m_TermsOfServiceURL))
			return;
		
		Application.OpenURL(m_TermsOfServiceURL);
	}

	void PrivacyPolicy()
	{
		if (string.IsNullOrEmpty(m_PrivacyPolicyURL))
			return; 
		
		Application.OpenURL(m_PrivacyPolicyURL);
	}

	async void Accept()
	{
		Processed = true;
		
		if (m_CompletionSource != null)
		{
			m_CompletionSource.TrySetResult(true);
			m_CompletionSource = null;
		}
		
		await HideAsync();
		
		m_MenuProcessor.RemoveMenu(MenuType.ConsentMenu);
	}
}