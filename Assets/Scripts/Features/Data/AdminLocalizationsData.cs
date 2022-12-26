using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Storage;
using UnityEngine.Purchasing;

public class AdminLocalizationsData : AdminData
{
	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Localizations"),
		new AdminSearchAttribute(""),
		new AdminCollectionAttribute("", "KEY_{0}"),
		new AdminFixedAttribute("{key}/{language_code}"),
		new AdminUpperAttribute("{key}/{language_code}"),
	};

	readonly string[] m_Languages;

	public AdminLocalizationsData(string[] _Languages)
	{
		m_Languages = _Languages;
	}

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{key}"))
			return CreateKey(_Node, _Path);
		return null;
	}

	AdminNode CreateKey(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		foreach (string language in m_Languages)
			AdminNode.Create(this, root, $"{_Path}/{language}", AdminNodeType.String);
		return root;
	}

	public override async Task LoadAsync()
	{
		Data = new Dictionary<string, object>();
		
		foreach (string language in m_Languages)
		{
			IDictionary<string, string> localization = await LoadLocalization(language);
			
			if (localization == null)
				continue;
			
			foreach (var entry in localization)
			{
				if (!Data.ContainsKey(entry.Key))
					Data[entry.Key] = new Dictionary<string, object>();
				
				if (Data[entry.Key] is not IDictionary<string, object> data)
					continue;
				
				data[language] = entry.Value;
			}
		}
		
		ProcessNodes();
	}

	public override async Task UploadAsync()
	{
		foreach (string language in m_Languages)
		{
			IDictionary<string, string> localization = new Dictionary<string, string>();
			
			foreach (object entry in Data.Values)
			{
				if (entry is not IDictionary<string, string> data)
					continue;
				
				localization[data["key"]] = data[language];
				
				string json = MiniJson.JsonEncode(localization);
				
				byte[] decode = Encoding.Unicode.GetBytes(json);
				
				byte[] encode = Compression.Compress(decode);
				
				StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Localization/{language}.lang");
				
				await reference.PutBytesAsync(encode);
			}
		}
	}

	static async Task<IDictionary<string, string>> LoadLocalization(string _Language)
	{
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child($"Localization/{_Language}.lang");
		
		byte[] file = await reference.GetBytesAsync(1024 * 1024 * 10);
		
		byte[] data = Compression.Decompress(file);
		
		string json = Encoding.Unicode.GetString(data);
		
		IDictionary<string, object> localization = MiniJson.JsonDecode(json) as IDictionary<string, object>;
		
		return localization?
			.ToDictionary(
			_Entry => _Entry.Key,
			_Entry => (string)_Entry.Value
		);
	}
}
