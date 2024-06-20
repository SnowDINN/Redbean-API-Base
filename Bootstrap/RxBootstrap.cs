using R3;
using Redbean.Api;

namespace Redbean;

public class RxBootstrap : IDisposable
{
	private static readonly CompositeDisposable disposables = new();
	
	public static void Setup()
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
	}

	public void Dispose()
	{
		disposables.Dispose();
		disposables.Clear();

		GC.SuppressFinalize(this);
	}
}