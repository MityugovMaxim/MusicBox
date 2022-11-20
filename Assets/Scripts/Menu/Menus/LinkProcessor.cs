using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Logging;
using Firebase.DynamicLinks;
using Firebase.Messaging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;
using Object = UnityEngine.Object;

[Preserve]
public class LinkProcessor
{
	const string SCHEME     = "audiobox";
	const string URL_PREFIX = "https://audiobox.page.link";
	const string URL_HOST   = "https://outofbounds.studio/audiobox?";

	[Inject] SignalBus          m_SignalBus;
	[Inject] DiscsParameter     m_DiscsParameter;
	[Inject] UrlProcessor       m_UrlProcessor;
	[Inject] StatisticProcessor m_StatisticProcessor;


	string m_AbsoluteLink;
	string m_DeepLink;
	string m_PushLink;
	string m_DynamicLink;
	string m_MessageLink;
	bool   m_Processing;
	Action m_LinkAction;

	public void Load()
	{
		m_AbsoluteLink = Application.absoluteURL;
		
		DynamicLinks.DynamicLinkReceived  += OnDynamicLink;
		FirebaseMessaging.MessageReceived += OnPushLink;
		Application.deepLinkActivated     += OnDeepLink;
		MessageProcessor.OnReceiveLink    += OnMessageLink;
		
		ProcessAdminMode();
		ProcessDevelopmentMode();
	}

	public void Subscribe(Action _Action) => m_LinkAction += _Action;

	public void Unsubscribe(Action _Action) => m_LinkAction -= _Action;

	public Task<string> GenerateSongLink(string _SongID)
	{
		return GenerateDynamicLink(
			"songs",
			new Dictionary<string, string>()
			{
				{ "song_id", _SongID }
			}
		);
	}

	public async Task<string> GenerateDynamicLink(string _Command, Dictionary<string, string> _Parameters = null)
	{
		if (_Command == null)
			_Command = string.Empty;
		
		StringBuilder builder = new StringBuilder();
		builder.Append(URL_HOST);
		builder.Append("command=").Append(_Command);
		
		string query = m_UrlProcessor.GetQuery(_Parameters);
		if (!string.IsNullOrEmpty(query))
			builder.Append('&').Append(query);
		
		DynamicLinkComponents components = new DynamicLinkComponents(new Uri(builder.ToString()), URL_PREFIX);
		components.IOSParameters     = new IOSParameters(Application.identifier);
		components.AndroidParameters = new AndroidParameters(Application.identifier);
		
		DynamicLinkOptions options = new DynamicLinkOptions();
		options.PathLength = DynamicLinkPathLength.Short;
		
		ShortDynamicLink link = await DynamicLinks.GetShortLinkAsync(components, options);
		
		foreach (string warning in link.Warnings)
			Log.Warning(this, warning);
		
		return link.Url.ToString();
	}

	void OnPushLink(object _Sender, MessageReceivedEventArgs _Args)
	{
		m_PushLink = _Args?.Message?.Link?.OriginalString;
		
		m_LinkAction?.Invoke();
	}

	void OnDynamicLink(object _Sender, ReceivedDynamicLinkEventArgs _Args)
	{
		m_DynamicLink = _Args?.ReceivedDynamicLink?.Url?.OriginalString;
		
		m_LinkAction?.Invoke();
	}

	void OnDeepLink(string _URL)
	{
		m_DeepLink = _URL;
		
		m_LinkAction?.Invoke();
	}

	void OnMessageLink(string _URL)
	{
		m_MessageLink = _URL;
	}

	void ProcessMode<T>(string _Mode) where T : Component
	{
		if (string.IsNullOrEmpty(_Mode))
			return;
		
		string[] links =
		{
			m_DynamicLink,
			m_MessageLink,
			m_PushLink,
			m_DeepLink,
			m_AbsoluteLink,
		};
		
		foreach (string link in links)
		{
			if (string.IsNullOrEmpty(link))
				continue;
			
			Uri url = CreateURI(link);
			
			if (url == null || url.Scheme != SCHEME || url.Host != "mode")
				continue;
			
			Dictionary<string, string> parameters = m_UrlProcessor.GetParameters(url.Query);
			
			if (!parameters.ContainsKey(_Mode))
				continue;
			
			T mode = Object.FindObjectOfType<T>(true);
			
			if (mode == null)
				return;
			
			mode.gameObject.SetActive(true);
		}
	}

	void ProcessAdminMode() => ProcessMode<AdminMode>("admin");

	void ProcessDevelopmentMode() => ProcessMode<DevelopmentMode>("development");

	public async Task Process(bool _Instant = false)
	{
		if (m_Processing)
			return;
		
		m_Processing = true;
		
		int discs = m_DiscsParameter.Value?.Count ?? 0;
		
		(string type, string link)[] links =
		{
			("dynamic", m_DynamicLink),
			("message", m_MessageLink),
			("push", m_PushLink),
			("deep", m_DeepLink),
			("absolute", m_AbsoluteLink),
			("fallback", discs == 0 ? $"{SCHEME}://play" : null),
		};
		
		ClearLinks();
		
		foreach (var entry in links)
		{
			if (await ProcessLink(entry.type, entry.link, _Instant))
				return;
		}
		
		m_Processing = false;
	}

	void ClearLinks()
	{
		m_AbsoluteLink = null;
		m_DynamicLink  = null;
		m_MessageLink  = null;
		m_PushLink     = null;
		m_DeepLink     = null;
	}

	async Task<bool> ProcessLink(string _Type, string _Link, bool _Instant)
	{
		if (string.IsNullOrEmpty(_Link))
			return false;
		
		Uri url = CreateURI(_Link);
		
		if (url == null || url.Scheme != SCHEME)
			return false;
		
		Log.Info(this, "Processing {0} link...", _Type);
		
		m_StatisticProcessor.LogLink(_Type, url.OriginalString);
		
		await m_UrlProcessor.ProcessURL(url.OriginalString, _Instant);
		
		return true;
	}

	Uri CreateURI(string _URL)
	{
		if (string.IsNullOrEmpty(_URL))
			return null;
		
		Uri url = new Uri(_URL, UriKind.Absolute);
		
		if (url.Scheme == SCHEME || string.IsNullOrEmpty(url.Query))
			return url;
		
		Dictionary<string, string> parameters = m_UrlProcessor.GetParameters(url.Query);
		
		if (parameters == null)
			return null;
		
		if (parameters.TryGetValue("command", out string command))
			parameters.Remove("command");
		
		if (string.IsNullOrEmpty(command))
			return null;
		
		StringBuilder builder = new StringBuilder();
		builder.Append(SCHEME);
		builder.Append("://");
		builder.Append(command);
		
		string query = m_UrlProcessor.GetQuery(parameters);
		if (!string.IsNullOrEmpty(query))
			builder.Append('?').Append(query);
		
		return new Uri(builder.ToString());
	}
}
