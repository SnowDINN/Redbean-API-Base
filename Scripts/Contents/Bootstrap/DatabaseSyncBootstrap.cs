using Redbean.Api;
using Redbean.Firebase;
using Redbean.Firebase.Storage;
using Redbean.Redis;

namespace Redbean;

public class DatabaseSyncBootstrap : IBootstrap
{
	public int ExecutionOrder => 30;

	public Task Setup()
	{
		FirebaseDatabase.GetConfigDocument("app").Listen(async _ =>
		{
			await RedisDatabase.SetValueAsync(RedisKey.APP_CONFIG, _.ToDictionary());
		});
		
		FirebaseDatabase.GetConfigDocument("table").Listen(async _ =>
		{
			var table = new Dictionary<string, string>();
			var objects = FirebaseStorage.GetFiles("Table/");
			foreach (var obj in objects)
			{
				var fileName = obj.Name.Split('/').Last();
				var tableName = fileName.Split('.').First();
			
				var text = await FirebaseStorage.DownloadTextFormatAsync(obj);
				table.Add(tableName, text);
			}

			await RedisDatabase.SetValueAsync(RedisKey.TABLE, new TableResponse { Table = table });
			await RedisDatabase.SetValueAsync(RedisKey.TABLE_CONFIG, _.ToDictionary());
		});

		return Task.CompletedTask;
	}
}