using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[Menu(MenuType.AdminMenu)]
public class UIAdminMenu : UIMenu
{
	class Bookmark
	{
		public float       Position { get; }
		public AdminNode[] Nodes    { get; }

		public Bookmark(float _Position, params AdminNode[] _Nodes)
		{
			Position = _Position;
			Nodes     = _Nodes;
		}
	}

	[SerializeField] UIAdminRoot m_Root;
	[SerializeField] Button      m_BackButton;
	[SerializeField] Button      m_UploadButton;
	[SerializeField] Button      m_RestoreButton;

	[Inject] MenuProcessor m_MenuProcessor;

	readonly Stack<Bookmark> m_Bookmarks = new Stack<Bookmark>();

	protected override void Awake()
	{
		base.Awake();
		
		m_BackButton.Subscribe(Back);
		m_UploadButton.Subscribe(Upload);
		m_RestoreButton.Subscribe(Restore);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_BackButton.Unsubscribe(Back);
		m_UploadButton.Unsubscribe(Upload);
		m_RestoreButton.Unsubscribe(Restore);
	}

	public void Select(params AdminNode[] _Nodes)
	{
		AdminNode[] nodes = m_Root.GetNodes();
		
		Bookmark bookmark = new Bookmark(m_Root.Y, nodes);
		
		m_Bookmarks.Push(bookmark);
		
		m_Root.Clear();
		m_Root.Y = 0;
		
		foreach (AdminNode node in _Nodes)
			m_Root.Add(node);
		
		m_Root.Rebuild();
	}

	protected override async void OnShowStarted()
	{
		base.OnShowStarted();
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async Task Refresh()
	{
		AdminLanguagesData languages = new AdminLanguagesData();
		
		await languages.LoadAsync();
		
		AdminData[] data =
		{
			new AdminRolesData(),
			new AdminAmbientData(),
			new AdminStoreData(),
			new AdminBannersData(),
			new AdminProductsData(languages.Languages),
			new AdminNewsData(languages.Languages),
			new AdminOffersData(languages.Languages),
			new AdminSongsData(),
			new AdminRevivesData(),
			new AdminDailyData(),
			new AdminProgressData(),
			new AdminDifficultyData(),
			new AdminChestsData(),
			new AdminVouchersData(),
			new AdminSeasonsData(),
			new AdminLocalizationsData(languages.Languages),
		};
		
		List<Task> tasks = new List<Task>();
		
		foreach (AdminData entry in data)
			tasks.Add(entry.LoadAsync());
		
		await Task.WhenAll(tasks);
		
		m_Bookmarks.Clear();
		
		m_Root.Clear();
		m_Root.Y = 0;
		
		foreach (AdminData entry in data)
			m_Root.Add(entry.Root);
		
		m_Root.Rebuild();
	}

	void Back()
	{
		if (m_Bookmarks.TryPop(out Bookmark bookmark))
		{
			m_Root.Clear();
			
			foreach (AdminNode node in bookmark.Nodes)
				m_Root.Add(node);
			
			m_Root.Rebuild();
			
			m_Root.Y = bookmark.Position;
		}
		else
		{
			Hide();
		}
	}

	async void Upload()
	{
		bool confirm = await m_MenuProcessor.ConfirmAsync(
			"admin_upload",
			"Upload",
			"Are you sure want to upload changed data?"
		);
		
		if (!confirm)
			return;
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		AdminData[] data = m_Root.GetAdminData();
		
		foreach (AdminData item in data)
			await item.UploadAsync();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}

	async void Restore()
	{
		AdminData[] data = m_Root.GetAdminData();
		
		foreach (AdminData item in data)
			await item.UploadAsync();
		
		await m_MenuProcessor.Show(MenuType.ProcessingMenu);
		
		await Refresh();
		
		await m_MenuProcessor.Hide(MenuType.ProcessingMenu);
	}
}
