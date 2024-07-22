namespace Redbean.Api
{
	public class StringRequest : IApiRequest
	{
		public StringRequest()
		{
			
		}

		public StringRequest(string value)
		{
			Value = value;
		}
		
		public string Value = string.Empty;
	}
}