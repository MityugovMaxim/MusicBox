using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AudioBox.Compression;
using Firebase.Storage;
using UnityEngine.Purchasing;

public abstract class AdminStorageData : AdminData
{
	protected abstract string Path { get; }

	protected abstract Encoding Encoding { get; }

	public override async Task LoadAsync()
	{
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(Path);
		
		byte[] file = await reference.GetBytesAsync(1024 * 1024 * 10);
		
		byte[] data = Compression.Decompress(file);
		
		string json = Encoding.GetString(data);
		
		Data = MiniJson.JsonDecode(json) as Dictionary<string, object>;
		
		ProcessNodes();
	}

	public override async Task UploadAsync()
	{
		string json = MiniJson.JsonEncode(Data);
		
		byte[] data = Encoding.GetBytes(json);
		
		byte[] file = Compression.Compress(data);
		
		StorageReference reference = FirebaseStorage.DefaultInstance.RootReference.Child(Path);
		
		await reference.PutBytesAsync(file);
	}
}
