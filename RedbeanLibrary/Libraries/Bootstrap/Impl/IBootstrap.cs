namespace Redbean;

public interface IBootstrap
{
	/// <summary>
	/// 실행 순서
	/// </summary>
	int ExecutionOrder { get; }
	
	/// <summary>
	/// 서버 실행 시 호출
	/// </summary>
	Task Setup();
}