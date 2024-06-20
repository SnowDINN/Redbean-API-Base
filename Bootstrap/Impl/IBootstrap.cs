namespace Redbean;

public interface IBootstrap : IDisposable
{
	Task Setup();
}