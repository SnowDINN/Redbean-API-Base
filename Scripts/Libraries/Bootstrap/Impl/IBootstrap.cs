namespace Redbean;

public interface IBootstrap : IDisposable
{
	int ExecutionOrder { get; }
	Task Setup();
}