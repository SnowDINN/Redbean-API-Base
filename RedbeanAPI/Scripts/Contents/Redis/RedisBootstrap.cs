using StackExchange.Redis;

namespace Redbean.Redis;

public class RedisBootstrap : IBootstrap
{
	public ConnectionMultiplexer Multiplexer { get; private set; }
	
	public int ExecutionOrder => 20;
	
	public async Task Setup()
	{
		Multiplexer = await ConnectionMultiplexer.ConnectAsync("host.docker.internal:6379");
		
		RedisDatabase.Initialize(Multiplexer.GetDatabase());
	}
}