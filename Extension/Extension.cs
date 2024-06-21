#pragma warning disable CS8602
#pragma warning disable CS8603

using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbean.Api;

namespace Redbean.Extension;

public static class Extension
{
#region MyRegion

	public static T ToConvert<T>(this string value) where T : IResponse =>
		JsonConvert.DeserializeObject<T>(value);

#endregion
	
#region Firestore Extension

	public static void AddListener(this FirestoreChangeListener listener, List<FirestoreChangeListener> list) =>
		list.Add(listener);
	
	public static object ToDocument<T>(this T value) =>
		JObjectToDictionary(JObject.Parse(JsonConvert.SerializeObject(value)));
	
	private static Dictionary<string, object> JObjectToDictionary(JObject? obj)
	{
		var result = new Dictionary<string, object>();

		foreach (var property in obj?.Properties()!)
			result[property.Name] = JTokenToObject(property.Value);

		return result;
	}

	private static object JTokenToObject(JToken? token)
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