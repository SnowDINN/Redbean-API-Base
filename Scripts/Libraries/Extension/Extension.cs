using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbean.Api;

namespace Redbean.Extension;

public static class Extension
{
#region Common

	public static T ToConvert<T>(this IDictionary<string, object> value) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
	
	public static T ToConvert<T>(this string value) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(value);

#endregion

#region Response

	public static ContentResult ToResponse(this string message, ApiErrorType type = 0) => Response.Return((int)type, message).ToResult();

	public static ContentResult ToResponse<T>(this IEnumerable<T> message, ApiErrorType type = 0) => Response.Return((int)type, message).ToResult();

	public static ContentResult ToResponse(this IDictionary<string, object> snapshot, ApiErrorType type = 0) => Response.Return((int)type, snapshot).ToResult();

	public static ContentResult ToResponse<T>(this T value, ApiErrorType type = 0) where T : IResponse => Response.Return((int)type, value).ToResult();
		
	private static ContentResult ToResult(this Response response)
	{
		return new ContentResult
		{
			Content = JsonConvert.SerializeObject(response),
			ContentType = "application/json"
		};
	}

#endregion
	
#region Firestore

	public static void AddListener(this FirestoreChangeListener listener, List<FirestoreChangeListener> list) =>
		list.Add(listener);
	
	public static object ToDocument<T>(this T value) =>
		JObjectToDictionary(JObject.Parse(JsonConvert.SerializeObject(value)));
	
	private static Dictionary<string, object> JObjectToDictionary(JObject obj)
	{
		var result = new Dictionary<string, object>();

		foreach (var property in obj?.Properties()!)
			result[property.Name] = JTokenToObject(property.Value);

		return result;
	}

	private static object JTokenToObject(JToken token)
	{
		switch (token.Type)
		{
			case JTokenType.Object:
				return JObjectToDictionary(token as JObject);
        
			case JTokenType.Array:
				return (from item in token as JArray select JTokenToObject(item)).ToList();
        
			default:
				return (token as JValue).Value;
		}
	}

#endregion
}