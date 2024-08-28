namespace Redbean.Api
{
	public class AuthenticationRequest : IApiRequest
	{
		public AuthenticationType type;
		public string id;
	}
}