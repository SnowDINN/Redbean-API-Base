using System;
using System.Collections.Generic;

namespace Redbean.Api
{
	public class StringResponse : IApiResponse
	{
		public StringResponse()
		{
			
		}

		public StringResponse(string value)
		{
			Value = value;
		}
		
		public string Value = string.Empty;
	}
	
	public class StringArrayResponse : IApiResponse
	{
		public StringArrayResponse()
		{
			
		}
		
		public StringArrayResponse(IEnumerable<string> value)
		{
			Value = value;
		}
		
		public IEnumerable<string> Value = Array.Empty<string>();
	}
}