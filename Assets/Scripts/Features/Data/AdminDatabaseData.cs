using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Purchasing;

public abstract class AdminDatabaseData : AdminData
{
	protected abstract string Path { get; }

	public override async Task LoadAsync()
	{
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(Path);
		
		DataSnapshot snapshot = await reference.GetValueAsync();
		
		string json = snapshot.GetRawJsonValue();
		
		Data = MiniJson.JsonDecode(json) as Dictionary<string, object>;
		
		if (Data == null)
			Data = new Dictionary<string, object>();
		
		ProcessNodes();
	}

	public override async Task UploadAsync()
	{
		string json = MiniJson.JsonEncode(Data);
		
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference.Child(Path);
		
		await reference.SetRawJsonValueAsync(json);
	}
}
