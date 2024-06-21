using R3;
using Redbean.Api;

namespace Redbean;

public class RxBootstrap : IBootstrap
{
	private static readonly CompositeDisposable disposables = new();
	public int ExecutionOrder => 30;

	public Task Setup()
	{
#region Refresh Token Expired Validation

		Observable.Interval(TimeSpan.FromSeconds(1))
			.Where(_ => Authorization.RefreshTokens.Count > 0).
			Subscribe(_ =>
			{
				var removes = (from token in Authorization.RefreshTokens
				               where token.Value.RefreshTokenExpire < DateTime.UtcNow
				               select token.Key).ToList();

				foreach (var remove in removes)
					Authorization.RefreshTokens.Remove(remove);
			}).AddTo(disposables);

#endregion
		
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		disposables.Dispose();
		disposables.Clear();

		GC.SuppressFinalize(this);
	}
}