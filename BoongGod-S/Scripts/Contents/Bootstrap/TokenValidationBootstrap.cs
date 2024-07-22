using R3;

namespace Redbean;

public class TokenValidationBootstrap : IBootstrap
{
	private static readonly CompositeDisposable disposables = new();
	public int ExecutionOrder => 40;

	public Task Setup()
	{
#region Refresh Token Expired Validation

		Observable.Interval(TimeSpan.FromSeconds(60))
			.Where(_ => AppToken.JwtTokens.Count > 0)
			.Subscribe(_ =>
			{
				var removes = (from token in AppToken.JwtTokens
				               where token.Value.RefreshTokenExpire < DateTime.UtcNow
				               select token.Key).ToList();

				foreach (var remove in removes)
					AppToken.JwtTokens.Remove(remove);
			}).AddTo(disposables);
		
		Observable.Interval(TimeSpan.FromSeconds(60))
			.Where(_ => AppToken.SwaggerSessionTokens.Count > 0)
			.Subscribe(_ =>
			{
				var removes = (from state in AppToken.SwaggerSessionTokens
				               where state.Value.Expire < DateTime.UtcNow
				               select state.Key).ToList();

				foreach (var remove in removes)
					AppToken.SwaggerSessionTokens.Remove(remove);
			}).AddTo(disposables);

#endregion
		
		return Task.CompletedTask;
	}
}