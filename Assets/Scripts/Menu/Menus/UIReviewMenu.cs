using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.ReviewMenu)]
public class UIReviewMenu : UIAnimationMenu
{
	const string REVIEW_PROCESSED_KEY = "REVIEW_PROCESSED";
	const string REVIEW_TIMESTAMP_KEY = "REVIEW_TIMESTAMP";

	static int ReviewCount { get; set; }

	public static bool Processed
	{
		get => PlayerPrefs.GetInt(REVIEW_PROCESSED_KEY, 0) > 0;
		private set => PlayerPrefs.SetInt(REVIEW_PROCESSED_KEY, value ? 1 : 0);
	}

	static long ReviewTimestamp
	{
		get => long.TryParse(PlayerPrefs.GetString(REVIEW_TIMESTAMP_KEY, "0"), out long value) ? value : 0;
		set => PlayerPrefs.SetString(REVIEW_TIMESTAMP_KEY, value.ToString());
	}

	[SerializeField] Button m_CancelButton;
	[SerializeField] Button m_ReviewButton;

	[Inject] MenuProcessor        m_MenuProcessor;
	[Inject] ConfigProcessor      m_ConfigProcessor;

	#if UNITY_ANDROID
	[Inject] ApplicationManager m_ApplicationManager;
	#endif

	RankType m_Rank;

	protected override void Awake()
	{
		base.Awake();
		
		m_CancelButton.onClick.AddListener(Cancel);
		m_ReviewButton.onClick.AddListener(Review);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_CancelButton.onClick.RemoveListener(Cancel);
		m_ReviewButton.onClick.RemoveListener(Review);
	}

	public void Setup(RankType _Rank)
	{
		m_Rank = _Rank;
	}

	public void Process()
	{
		if (Processed)
			return;
		
		ReviewCount++;
		
		if (ReviewCount < m_ConfigProcessor.ReviewCount)
			return;
		
		int rank = (int)m_Rank;
		
		if (rank < m_ConfigProcessor.ReviewRank)
			return;
		
		long timestamp = TimeUtility.GetTimestamp();
		long cooldown  = m_ConfigProcessor.ReviewCooldown * 1000;
		
		if (timestamp - ReviewTimestamp < cooldown)
			return;

		ReviewTimestamp = timestamp;
		
		Show();
	}

	async void Cancel()
	{
		await HideAsync();
		
		m_MenuProcessor.RemoveMenu(MenuType.ReviewMenu);
	}

	async void Review()
	{
		Processed = true;
		
		#if UNITY_IOS
		AppStoreReview();
		#elif UNITY_ANDROID
		GooglePlayReview();
		#endif
		
		await HideAsync();
		
		m_MenuProcessor.RemoveMenu(MenuType.ReviewMenu);
	}

	#if UNITY_IOS
	void AppStoreReview()
	{
		UnityEngine.iOS.Device.RequestStoreReview();
	}
	#endif

	#if UNITY_ANDROID
	void GooglePlayReview()
	{
		string url = m_ApplicationManager.GetReviewURL();
		
		if (string.IsNullOrEmpty(url))
			return;
		
		Application.OpenURL(url);
	}
	#endif
}
