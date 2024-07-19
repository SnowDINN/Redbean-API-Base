using System.Text;
using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Redbean.Api;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Redbean.Firebase.Storage;

public class FirebaseStorage
{
	private static StorageClient storage;
	
	public static void Initialize(StorageClient storage) => FirebaseStorage.storage = storage;

	public static PagedEnumerable<Objects, Object> GetFiles(string path) => 
		storage.ListObjects(FirebaseSetting.StorageBucket, path);
	
	public static async Task DeleteFilesAsync(PagedEnumerable<Objects, Object> Objects)
	{
		foreach (var obj in Objects)
			await storage.DeleteObjectAsync(FirebaseSetting.StorageBucket, obj.Name);
	}
	
	public static async Task UploadFilesAsync(string path, IEnumerable<RequestFile> files)
	{
		foreach (var file in files)
			await UploadFileAsync(path, file);
	}

	public static async Task UploadFileAsync(string path, RequestFile file)
	{
		var obj = new Object
		{
			Bucket = FirebaseSetting.StorageBucket,
			Name = $"{path}{file.FileName}",
			CacheControl = "no-store",
		};

		await storage.UploadObjectAsync(obj, new MemoryStream(file.FileData));
	}

	public static async Task<string> DownloadTextFormatAsync(Object obj)
	{
		using var memoryStream = new MemoryStream();
		await storage.DownloadObjectAsync(obj, memoryStream);

		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}