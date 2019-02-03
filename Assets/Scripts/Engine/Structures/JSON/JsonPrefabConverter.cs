using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Instantiates a prefab whenever a MonoBehaviour is encountered and populates the
// instance with the state provided in JSON. You have to provide path to the prefab.
// Anything enclosed with {} in the prefab path is replaced with actual value of the
// property. You can use this to instantiate different prefabs depending on the state.

// Example
// This will instantiate two prefabs, Building/ProducerBuilding and Building/BankBuilding,
// with Position (1, 1, 1) and (2, 2, 2) respectively.
//
// C#:
// [JsonConverter(typeof(JsonPrefabConverter), "Buildings/{Type}")]
// [JsonObject(MemberSerialization.OptIn)]
// public class Building: MonoBehaviour
//
// Json:
// {
// "Buildings":
// [
//	 {
// 		"Type": "ProducerBuilding",
// 		"Position": {"x": 1, "y": 1, "z": 1}
//	 },
//	 {
//	 	"Type": "BankBuilding",
//	 	"Position": {"x": 2, "y": 2, "z": 2}
//	 }
// ]
// }

public class JsonPrefabConverter : JsonConverter
{
	public string Path;

	public JsonPrefabConverter(string path)
	{
		Path = path;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jObject = JToken.ReadFrom(reader);
		string path = ReplaceValues(jObject);

		var prefab = Resources.Load(path, objectType);
		if (prefab == null)
			return null;

		var instance = UnityEngine.Object.Instantiate(prefab);
		instance.name = prefab.name;
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, instance);

		return instance;
	}

	protected string ReplaceValues(JToken jObject)
	{
		string path = Path;

		int startIndex = 0;
		while (true)
		{
			int braceOpenIndex = path.IndexOf('{', startIndex);
			if (braceOpenIndex >= 0)
			{
				int braceEndIndex = path.IndexOf('}', braceOpenIndex + 1);
				if (braceEndIndex >= 0)
				{
					string typeProperty = path.Slice(braceOpenIndex + 1, braceEndIndex);
					string typePropertyValue = jObject[typeProperty].Value<string>();
					path = path.Left(braceOpenIndex) + typePropertyValue + path.Right(braceEndIndex + 1);
					startIndex = braceEndIndex + 1;
				}
				else
					break;
			}
			else
				break;
		}

		return path;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override bool CanConvert(Type objectType)
	{
		throw new NotImplementedException();
	}

	public override bool CanWrite => false;
}