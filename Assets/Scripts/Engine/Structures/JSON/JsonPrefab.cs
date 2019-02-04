using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// TODO: Save state  in Prefab mode

/// <summary>
/// Instantiates a prefab whenever a MonoBehaviour is encountered and populates the
/// instance with a state provided in JSON. You have to provide path to the prefab.
/// Anything enclosed with {} in the prefab path is replaced with the actual value of 
/// a property, so you can instantiate different prefabs depending on the state.
/// 
/// There are two ways to use this class, the Converter method or the attribute method.
/// In the converter method, you put a JsonConverter attribute on a MonoBehaviour with
/// type JsonPrefabConverter and the prefab path as its first argument. On the Json side,
/// you use the actual MonoBehaviour type in the GameState object. Whenever the Json is
/// loaded, the converter will instantiate objects and assign them in the GameState. This
/// way the MonoBehaviours will be strongly bound to the GameState. The advantage of this
/// is that you can change MonoBehaviours and they'll automatically be reflected in the
/// GameState. The disavantage being if MonoBehaviours are destroyed, they'll become null
/// or inaccesible in the GameState.
/// 
/// In the attribute mode, you put a JsonPrefab attribute on a separate class denoting a 
/// MonoBehaviour's state and use that in the GameState. The Json will be loaded normally,
/// and nothing will happen by itself. To instantiate objects, you have to call
/// JsonPrefab.Instantiate on state objects whenever you want. The advantage of this is
/// that you have more control on the life-cycle of MonoBehaviours and states will not
/// become null if MonoBehaviours are destroyed. The disadvantage is that you have to save
/// back state manually if/when MonoBehaviours are changed. To aid this, there is a method
/// called JsonPrefab.Save which you can call manually and is automatically called
/// when a Json-created MonoBehaviour is destroyed.
/// </summary>
/// 
/// <example>
/// This will instantiate two prefabs, Building/ProducerBuilding and Building/BankBuilding,
/// with Position (1, 1) and (2, 2) respectively.
///
/// C# (Converter method):
/// <code>
/// [JsonConverter(typeof(JsonPrefabConverter), "Buildings/{Type}")]
/// [JsonObject(MemberSerialization.OptIn)]
/// public class Building: MonoBehaviour
/// {
///		[JsonProperty]
///		public string Type;
///		
/// 	[JsonProperty]
///		public Vector2 Position;
/// }
/// 
/// public class GameState
/// {
///		public List<Building> Buildings;
/// }
/// </code>
/// 
/// C# (Attribute method):
/// <code>
/// [JsonObject(MemberSerialization.OptIn)]
/// public class Building: MonoBehaviour
/// {
///		[JsonProperty]
///		public string Type;
///		
/// 	[JsonProperty]
///		public Vector2 Position;
/// }
/// 
/// [Prefab("Buildings/{Type}")]
/// public class GameState
/// {
///		public List<BuildingState> Buildings;
/// }
/// 
/// public class BuildingState
/// {
///		public string Type;
///		public Vector2 Position;
/// }
/// 
/// JsonPrefab.Instantiate<Building>(GameState.Buildings);
/// </code>
/// 
/// Json:
/// <code>
/// {
///		"Buildings":
///		[
///			{
/// 			"Type": "ProducerBuilding",
/// 			"Position": {"x": 1, "y": 1}
///			},
///			{
///	 			"Type": "BankBuilding",
///	 			"Position": {"x": 2, "y": 2}
///			}
///		]
/// }
/// </code>
/// </example>
/// 

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
		string path = JsonPrefab.ReplaceValues(Path, jObject);

		var prefab = Resources.Load(path, objectType);
		if (prefab == null)
			return null;

		var instance = UnityEngine.Object.Instantiate(prefab);
		instance.name = prefab.name;
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, instance);

		return instance;
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

public class JsonPrefab
{
	protected static JsonSerializer serializer = JsonSerializer.CreateDefault();

	public static List<T> Instantiate<T>(IEnumerable states) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		foreach (var state in states)
		{
			var instance = Instantiate<T>(state);
			if (instance != null)
				list.Add(instance);
		}
		return list;
	}



	public static T Instantiate<T>(object state) where T : MonoBehaviour
	{
		Type type = state.GetType();
		JsonPrefabAttribute attribute = type.GetCustomAttributes(typeof(JsonPrefabAttribute), true).Cast<JsonPrefabAttribute>().FirstOrDefault();
		if (attribute == null)
			return null;

		JObject jObject = JObject.FromObject(state);
		string currentPath = ReplaceValues(attribute.Path, jObject);

		var prefab = ResourceManager.Load<T>(ResourceFolder.Resources, currentPath);
		if (prefab == null)
			return null;

		var instance = UnityEngine.Object.Instantiate(prefab);
		instance.name = prefab.name;
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, instance);

		var serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveOnDestroy>();
		serializeOnDestroy.MonoObject = instance;
		serializeOnDestroy.JsonObject = state;

		return instance;
	}

	/*
	public static T Instantiate<T>(string path, JObject jObject) where T : MonoBehaviour
	{
		string currentPath = ReplaceValues(path, jObject);

		var prefab = ResourceManager.Load<T>(ResourceFolder.Resources, currentPath);
		if (prefab == null)
			return null;

		var instance = UnityEngine.Object.Instantiate(prefab);
		instance.name = prefab.name;
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, instance);

		var serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveOnDestroy>();
		serializeOnDestroy.MonoObject = instance;
		serializeOnDestroy.JsonObject = state;

		return instance;
	}
	*/

	public static void Save(List<MonoBehaviour> monos, List<object> states)
	{
		if (monos.Count != states.Count)
			return;

		for (int i = 0; i < monos.Count; i++)
			Save(monos[i], states[i]);
	}

	public static void Save(MonoBehaviour mono, object state)
	{
		JObject jObject = JObject.FromObject(mono);
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, state);
	}

	public static string ReplaceValues(string path, JToken jObject)
	{
		string currentPath = path;
		int startIndex = 0;
		while (true)
		{
			int braceOpenIndex = currentPath.IndexOf('{', startIndex);
			if (braceOpenIndex >= 0)
			{
				int braceEndIndex = currentPath.IndexOf('}', braceOpenIndex + 1);
				if (braceEndIndex >= 0)
				{
					string typeProperty = currentPath.Slice(braceOpenIndex + 1, braceEndIndex);
					string typePropertyValue = jObject[typeProperty].Value<string>();
					currentPath = currentPath.Left(braceOpenIndex) + typePropertyValue + currentPath.Right(braceEndIndex + 1);
					startIndex = braceEndIndex + 1;
				}
				else
					break;
			}
			else
				break;
		}

		return currentPath;
	}
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class JsonPrefabAttribute : Attribute
{
	public string Path;

	public JsonPrefabAttribute(string path)
	{
		Path = path;
	}
}

public class JsonSaveOnDestroy : MonoBehaviour
{
	public MonoBehaviour MonoObject;
	public object JsonObject;

	protected void OnDestroy()
	{
		JsonPrefab.Save(MonoObject, JsonObject);
	}
}