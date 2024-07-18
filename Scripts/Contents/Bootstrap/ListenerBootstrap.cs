using System.Text;
using Google.Cloud.Firestore;
using Redbean.Api;
using Redbean.Extension;
using Redbean.Firebase;
using Redbean.Redis;

namespace Redbean;

public class ListenerBootstrap : IBootstrap
{
	private readonly List<FirestoreChangeListener> listeners = [];
	public int ExecutionOrder => 20;

	public async Task Setup()
	{
		await RedisContainer.Initialize();
		
		FirebaseSetting.AppConfigDocument.Listen(async _ =>
		{
			await RedisContainer.SetValueAsync(RedisKey.APP_CONFIG, _.ToDictionary());
		}).AddTo(listeners);
		
		FirebaseSetting.TableConfigDocument.Listen(async _ =>
		{
			var table = new Dictionary<string, string>();
			var objects = FirebaseSetting.Storage?.ListObjects(FirebaseSetting.StorageBucket, "Table/");
			foreach (var obj in objects)
			{
				using var memoryStream = new MemoryStream();
				var tableFile = await FirebaseSetting.Storage?.DownloadObjectAsync(obj, memoryStream);

				var fileName = tableFile.Name.Split('/').Last();
				var tableName = fileName.Split('.').First();
			
				table.Add(tableName, Encoding.UTF8.GetString(memoryStream.ToArray()));
			}

			await RedisContainer.SetValueAsync(RedisKey.TABLE, new TableResponse { Table = table });
			await RedisContainer.SetValueAsync(RedisKey.TABLE_CONFIG, _.ToDictionary());
		}).AddTo(listeners);
	}

	public async void Dispose()
	{
		await RedisContainer.Multiplexer.DisposeAsync();
		
		foreach (var listener in listeners)
			await listener.StopAsync();
		
		listeners.Clear();
		
		GC.SuppressFinalize(this);
	}
}