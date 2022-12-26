using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Purchasing;

public class AdminDescriptorData : AdminData
{
	protected override AdminAttribute[] Attributes { get; } =
	{
		new AdminTitleAttribute("", "Descriptors"),
		new AdminFixedAttribute("{language}"),
		new AdminUpperAttribute("{language}"),
		new AdminFixedAttribute("{language}/{value}"),
		new AdminAreaAttribute("{language}/description"),
		new AdminHideAttribute("{language}/order"),
	};

	readonly string   m_Descriptor;
	readonly string   m_ID;
	readonly string[] m_Languages;

	public AdminDescriptorData(string _Descriptor, string _ID, params string[] _Languages)
	{
		m_Descriptor = _Descriptor;
		m_ID         = _ID;
		m_Languages  = _Languages;
	}

	public override AdminNode CreateObject(AdminNode _Node, string _Path)
	{
		if (AdminUtility.Match(_Path, "{language}"))
			return CreateDescriptor(_Node, _Path);
		return null;
	}

	public async void Load()
	{
		Data = new Dictionary<string, object>();
		
		foreach (string language in m_Languages)
			Data[language] = new Dictionary<string, object>();
		
		ProcessNodes();
		
		await LoadAsync();
	}

	public override async Task LoadAsync()
	{
		Data = new Dictionary<string, object>();
		
		foreach (string language in m_Languages)
		{
			string path = $"{m_Descriptor}/{language}/{m_ID}";
			
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			
			DataSnapshot snapshot = await reference.GetValueAsync();
			
			string json = snapshot.GetRawJsonValue();
			
			IDictionary<string, object> data = MiniJson.JsonDecode(json) as IDictionary<string, object>;
			
			Data[language] = data;
		}
		
		ProcessNodes();
	}

	public override async Task UploadAsync()
	{
		foreach (string language in m_Languages)
		{
			string path = $"{m_Descriptor}/{language}/{m_ID}";
			
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
			
			string json = MiniJson.JsonEncode(Data[language]);
			
			await reference.SetRawJsonValueAsync(json);
		}
	}

	AdminNode CreateDescriptor(AdminNode _Node, string _Path)
	{
		AdminNode root = AdminNode.Create(this, _Node, _Path, AdminNodeType.Object);
		AdminNode.Create(this, root, $"{_Path}/title", AdminNodeType.String);
		AdminNode.Create(this, root, $"{_Path}/description", AdminNodeType.String);
		return root;
	}
}
