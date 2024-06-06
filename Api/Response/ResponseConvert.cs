using Newtonsoft.Json;

namespace BoongGod.Api;

public class ResponseConvert
{
	public static string ToResult<T>(T value, int code = 0)
	{
		var result = new ResponseResult<T>
		{
			StatusCode = code,
			Result = value
		};

		return JsonConvert.SerializeObject(result, Formatting.Indented);
	}
}

public class ResponseResult<T>
{
	public int StatusCode;
	public T Result;
}