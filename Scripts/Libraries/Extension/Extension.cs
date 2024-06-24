using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbean.Api;
using Redbean.Api.Controllers;

namespace Redbean.Extension;

public static class Extension
{
#region Common

	/// <summary>
	/// 클래스 변환
	/// </summary>
	public static T ToConvert<T>(this IDictionary<string, object> value) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
	
	/// <summary>
	/// 클래스 변환
	/// </summary>
	public static T ToConvert<T>(this string value) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(value);

#endregion

#region Response

	/// <summary>
	/// API 데이터 반환
	/// </summary>
	public static ContentResult ToResponse(this string message, ApiErrorType type = 0) => Response.Return((int)type, message).ToResult();

	/// <summary>
	/// API 데이터 반환
	/// </summary>
	public static ContentResult ToResponse<T>(this IEnumerable<T> message, ApiErrorType type = 0) => Response.Return((int)type, message).ToResult();

	/// <summary>
	/// API 데이터 반환
	/// </summary>
	public static ContentResult ToResponse(this IDictionary<string, object> snapshot, ApiErrorType type = 0) => Response.Return((int)type, snapshot).ToResult();

	/// <summary>
	/// API 데이터 반환
	/// </summary>
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

	/// <summary>
	/// Firestore 리스너 구독
	/// </summary>
	public static void Subscribe(this FirestoreChangeListener listener, List<FirestoreChangeListener> list) =>
		list.Add(listener);
	
	/// <summary>
	/// Firestore 데이터화
	/// </summary>
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

#region User

	public static async Task<UserResponse> GetRequestUser(this HttpRequest request)
	{
		var body = Authorization.GetAuthorizationBody(request);
		return await Redis.GetUserAsync(body.UserId);
	}

#endregion
}