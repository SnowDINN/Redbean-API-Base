#pragma warning disable CS8602
#pragma warning disable CS8603

using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Redbean.Extension;

public static class Extension
{
	public static T? ToConvert<T>(this DocumentSnapshot snapshot) => 
		JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(snapshot.ToDictionary()));

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
}