using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

/// <summary>
/// Instantiates a prefab whenever a MonoBehaviour is encountered and populates the
/// instance with a state provided in JSON. You have to provide path to the prefab.
/// Anything enclosed with {} in the prefab path is replaced with the actual value of
/// a property, so you can instantiate different prefabs depending on the state.
///
/// There are three ways to use this class – Converter, State-object or JObject method.
/// In the converter mode, you put a JsonConverter attribute on a MonoBehaviour with
/// type JsonPrefabConverter and the prefab path as its first argument. On the Json side,
/// you use the actual MonoBehaviour type in the GameState object. Whenever the Json is
/// loaded, the converter will instantiate objects and assign them in the GameState. This
/// way, the MonoBehaviours will be strongly bound to the GameState. The advantage of this
/// is that you can change MonoBehaviours and they'll automatically be reflected. The
/// disadvantage being if MonoBehaviours are destroyed, they'll become null or inaccessible
/// in the GameState.
///
/// In the state-object mode, you put JsonPrefab attribute on a separate class denoting a
/// MonoBehaviour's state and use that in the GameState. The Json will be loaded normally,
/// and nothing will happen by itself. To instantiate objects, you have to call
/// JsonPrefab.Instantiate on state-objects whenever you want. The advantage of this is
/// that you have more control on the life-cycle of MonoBehaviours and states will not
/// become null if MonoBehaviours are destroyed. The disadvantage is that you have to save
/// back state manually if/when MonoBehaviours are changed. To aid this, there is a method
/// called JsonPrefab.Save which can be called manually and is automatically called when a
/// Json-created MonoBehaviour is destroyed. This is the slowest method since we have to
/// convert to and from JObject each time we have to populate data.
///
/// The JObject mode is very similar to state-object mode, except that you put JObject in
/// GameState wherever you want to work with MonoBehaviours and call JsonPrefab.Instantiate
/// by providing it the prefab path and JObjects to instantiate directly. This is the faster
/// method and doesn't have problems like having to use JsonSubtypes to create the correct
/// State-object type.
/// </summary>
///
/// <example>
/// The following will all instantiate two prefabs, "Building/ProducerBuilding" and
/// "Building/BankBuilding", with Position (1, 1) and (2, 2) respectively.
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
///
///
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
///		public List&lt;Building&gt; Buildings;
/// }
/// </code>
///
///
///
/// C# (State-object method):
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
///		public List&lt;BuildingState&lt; Buildings;
/// }
///
/// public class BuildingState
/// {
///		public string Type;
///		public Vector2 Position;
/// }
///
/// JsonPrefab.Instantiate&lt;Building&lt;(GameState.Buildings);
/// </code>
///
///
///
/// C# (JObject method):
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
/// public class GameState
/// {
///		public List&lt;JObject&lt; Buildings;
/// }
///
/// JsonPrefab.Instantiate&lt;Building&lt;("Buildings/{Type}", GameState.Buildings);
/// </code>
/// </example>

public class JsonPrefabConverter : JsonConverter
{
	public readonly string Path;

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

		var instance = Object.Instantiate(prefab);
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
		return typeof(MonoBehaviour).IsAssignableFrom(objectType);
	}

	public override bool CanWrite => false;
}

public static class JsonPrefab
{
	private static JsonSerializer serializer = JsonSerializer.CreateDefault();

	public static List<T> Instantiate<T>(IEnumerable stateObjects, bool saveOnDestroy = true) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		foreach (var stateObject in stateObjects)
		{
			var instance = Instantiate<T>(stateObject, saveOnDestroy);
			if (instance != null)
				list.Add(instance);
		}
		return list;
	}

	public static List<T> Instantiate<T>(string path, IEnumerable<JObject> jObjects, bool saveOnDestroy = true) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		foreach (var jObject in jObjects)
		{
			var instance = Instantiate<T>(path, jObject, saveOnDestroy);
			if (instance != null)
				list.Add(instance);
		}
		return list;
	}

	public static T Instantiate<T>(object stateObject, bool saveOnDestroy = true) where T : MonoBehaviour
	{
		Type type = stateObject.GetType();
		JsonPrefabAttribute attribute = type.GetCustomAttributes(typeof(JsonPrefabAttribute), true).Cast<JsonPrefabAttribute>().FirstOrDefault();
		if (attribute == null)
			return null;

		JObject jObject = JObject.FromObject(stateObject);
		var instance = InstantiateInternal<T>(attribute.Path, jObject);

		if (instance == null || !saveOnDestroy)
			return instance;

		var serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveStateOnDestroy>();
		serializeOnDestroy.MonoObject = instance;
		serializeOnDestroy.StateObject = stateObject;

		return instance;
	}

	public static T Instantiate<T>(string path, JObject jObject, bool saveOnDestroy = true) where T : MonoBehaviour
	{
		var instance = InstantiateInternal<T>(path, jObject);
		if (instance == null || !saveOnDestroy)
			return instance;

		var serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveJObjectOnDestroy>();
		serializeOnDestroy.MonoObject = instance;
		serializeOnDestroy.JObject = jObject;
		return instance;
	}

	private static T InstantiateInternal<T>(string path, JObject jObject) where T : MonoBehaviour
	{
		string currentPath = ReplaceValues(path, jObject);

		var prefab = ResourceManager.Load<T>(ResourceFolder.Resources, currentPath);
		if (prefab == null)
			return null;

		var instance = Object.Instantiate(prefab);
		instance.name = prefab.name;
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, instance);

		return instance;
	}

	public static void Save<T>(List<T> monoObjects, List<object> stateObjects) where T: MonoBehaviour
	{
		if (monoObjects.Count != stateObjects.Count)
			return;

		for (int i = 0; i < monoObjects.Count; i++)
			Save(monoObjects[i], stateObjects[i]);
	}

	public static void Save<T>(List<T> monoObjects, List<JObject> jObjects) where T : MonoBehaviour
	{
		if (monoObjects.Count != jObjects.Count)
			return;

		for (int i = 0; i < monoObjects.Count; i++)
			Save(monoObjects[i], jObjects[i]);
	}

	public static void Save(MonoBehaviour monoObject, object stateObject)
	{
		JObject jObject = JObject.FromObject(monoObject);
		using (var jObjectReader = jObject.CreateReader())
			serializer.Populate(jObjectReader, stateObject);
	}

	public static void Save(MonoBehaviour monoObject, JObject jObject)
	{
		JObject newJObject = JObject.FromObject(monoObject);
		jObject.ReplaceAll(newJObject.Children());
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
					currentPath = currentPath.Left(braceOpenIndex) + typePropertyValue + currentPath.Slice(braceEndIndex + 1);
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
	public readonly string Path;

	public JsonPrefabAttribute(string path)
	{
		Path = path;
	}
}

public class JsonSaveStateOnDestroy : MonoBehaviour
{
	public MonoBehaviour MonoObject;
	public object StateObject;

	protected void OnDestroy()
	{
		JsonPrefab.Save(MonoObject, StateObject);
	}
}

public class JsonSaveJObjectOnDestroy : MonoBehaviour
{
	public MonoBehaviour MonoObject;
	public JObject JObject;

	protected void OnDestroy()
	{
		JsonPrefab.Save(MonoObject, JObject);
	}
}