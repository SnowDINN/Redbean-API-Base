using R3;

namespace Redbean;

public class RxBootstrap : IBootstrap
{
	private static readonly CompositeDisposable disposables = new();
	public int ExecutionOrder => 30;

	public Task Setup()
	{
#region Refresh Token Expired Validation

		Observable.Interval(TimeSpan.FromSeconds(60))
			.Where(_ => JwtAuthentication.Tokens.Count > 0)
			.Subscribe(_ =>
			{
				var removes = (from token in JwtAuthentication.Tokens
				               where token.Value.RefreshTokenExpire < DateTime.UtcNow
				               select token.Key).ToList();

				foreach (var remove in removes)
					JwtAuthentication.Tokens.Remove(remove);
			}).AddTo(disposables);
		
		Observable.Interval(TimeSpan.FromSeconds(60))
			.Where(_ => GoogleAuthentication.Tokens.Count > 0)
			.Subscribe(_ =>
			{
				var removes = (from state in GoogleAuthentication.Tokens
				               where state.Value.Expire < DateTime.UtcNow
				               select state.Key).ToList();

				foreach (var remove in removes)
					GoogleAuthentication.Tokens.Remove(remove);
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