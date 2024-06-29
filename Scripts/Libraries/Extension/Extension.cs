using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbean.Api;

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
	
	public static ContentResult ToPublish<T>(this T value, int errorCode = 0) where T : IResponse
	{
		var response = Response.Default;
		response.ErrorCode = errorCode;
		response.Value = value;
		
		if (errorCode == 0)
			return new ContentResult
			{
				Content = JsonConvert.SerializeObject(response),
				ContentType = "application/json"
			};
		
		response.Value = default;
		return new ContentResult
		{
			Content = JsonConvert.SerializeObject(response),
			ContentType = "application/json"
		};

	}

	public static ContentResult ToPublishCode(this ControllerBase controllerBase, int errorCode = 0)
	{
		var response = Response.Default;
		response.ErrorCode = errorCode;
		response.Value = default;
		
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

	public static async Task<UserResponse> GetRequestUser(this HttpRequest request) =>
		await Redis.GetUserAsync(Authorization.GetUserId(request));

#endregion
}