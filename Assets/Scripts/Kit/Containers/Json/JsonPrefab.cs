using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kit.Containers
{
	/// <summary>
	///     Instantiates a prefab for each object encountered while reading a Json and populates the instances with the states
	///     provided. You have to provide the path to a prefab and anything enclosed with {} in the prefab path is replaced with the
	///     value of a property, so you can instantiate different prefabs depending on the state.
	/// </summary>
	/// <remarks>
	///     <para>There are three ways to use this class – Mono-only, State-Mono or JObject-Mono method:</para>
	///     <list type="number">
	///         <item>
	///             <description>
	///                 <para>
	///                     In the Mono-only mode, you put a <see cref="JsonConverter" /> attribute on a <see cref="MonoBehaviour" />
	///                     with type <see cref="JsonPrefabConverter" /> and the prefab path as its first argument. On the Json side, you
	///                     use the actual <see cref="MonoBehaviour" /> type in the GameState object. Whenever the Json is loaded, the
	///                     converter will instantiate objects and assign them in the GameState. This way, the
	///                     <see cref="MonoBehaviour" />s will be strongly bound to the GameState. The advantage of this is that any
	///                     changes in the <see cref="MonoBehaviour" />s will be automatically be reflected in the state. The
	///                     disadvantage being if <see cref="MonoBehaviour" />s are destroyed, they'll become <see langword="null" /> or
	///                     inaccessible in the GameState.
	///                 </para>
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <para>
	///                     In the State-Mono mode, you put <see cref="JsonPrefabAttribute" /> on a separate class denoting a
	///                     <see cref="MonoBehaviour" />'s state and use that in the GameState. The Json will be loaded normally, and
	///                     nothing will happen by itself. To instantiate objects, you have to call
	///                     <see cref="JsonPrefab.Instantiate{T}(IEnumerable, bool)" /> on state-objects whenever you want. The advantage
	///                     of this is that you have more control on the life-cycle of MonoBehaviours and states will not become
	///                     <see langword="null" /> if <see cref="MonoBehaviour" />s are destroyed. The disadvantage is that you have to
	///                     save back state manually if/when <see cref="MonoBehaviour" />s are changed. To aid this, there is a method
	///                     called <see cref="JsonPrefab.Save(MonoBehaviour, object)" /> which can be called manually and is
	///                     automatically called when a Json-created <see cref="MonoBehaviour" /> is destroyed. This is the slowest
	///                     method since we have to convert to and from <see cref="JObject" /> each time we have to populate data.
	///                 </para>
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <para>
	///                     The JObject mode is very similar to State-Mono mode, except that you put <see cref="JObject" /> in GameState
	///                     wherever you want to work with <see cref="MonoBehaviour" />s and call
	///                     <see
	///                         cref="JsonPrefab.Instantiate{T}(string, System.Collections.Generic.IEnumerable{Newtonsoft.Json.Linq.JObject}, bool)" />
	///                     by providing it the prefab path and <see cref="JObject" />s to instantiate directly. This is the faster
	///                     method and doesn't have problems like having to use <see cref="JsonSubtypes" /> to create the correct
	///                     State-object type.
	///                 </para>
	///             </description>
	///         </item>
	///     </list>
	/// </remarks>
	/// <example>
	///     The following will all instantiate two prefabs, "Building/ProducerBuilding" and "Building/BankBuilding", with Position (1, 1)
	///     and (2, 2) respectively.
	///     <code language="javascript" title="Json">
	///   {
	///  		"Buildings":
	///  		[
	///  			{
	///   			"Type": "ProducerBuilding",
	///   			"Position": {"x": 1, "y": 1}
	///  			},
	///  			{
	///  	 			"Type": "BankBuilding",
	///  	 			"Position": {"x": 2, "y": 2}
	///  			}
	///  		]
	///   }
	///   </code>
	///     <code title="Mono-only">
	///   [JsonConverter(typeof(JsonPrefabConverter), "Buildings/{Type}")]
	///   [JsonObject(MemberSerialization.OptIn)]
	///   public class Building: MonoBehaviour
	///   {
	///  		[JsonProperty]
	///  		public string Type;
	/// 
	///   		[JsonProperty]
	///  		public Vector2 Position;
	///   }
	///   public class GameState
	///   {
	///  		public List&lt;Building&gt; Buildings;
	///   }
	///   </code>
	///     <code title="State-Mono">
	///   [JsonObject(MemberSerialization.OptIn)]
	///   public class Building: MonoBehaviour
	///   {
	///  		[JsonProperty]
	///  		public string Type;
	/// 
	///   		[JsonProperty]
	///  		public Vector2 Position;
	///   }
	///   [JsonPrefab("Buildings/{Type}")]
	///   public class GameState
	///   {
	///  		public List&lt;BuildingState&gt; Buildings;
	///   }
	///   public class BuildingState
	///   {
	///  		public string Type;
	///  		public Vector2 Position;
	///   }
	///   JsonPrefab.Instantiate&lt;Building&gt;(GameState.Buildings);
	///   </code>
	///     <code title="JObject-Mono">
	///   [JsonObject(MemberSerialization.OptIn)]
	///   public class Building: MonoBehaviour
	///   {
	///  		[JsonProperty]
	///  		public string Type;
	/// 
	///   		[JsonProperty]
	///  		public Vector2 Position;
	///   }
	///   public class GameState
	///   {
	///  		public List&lt;JObject&gt; Buildings;
	///   }
	///   JsonPrefab.Instantiate&lt;Building&gt;("Buildings/{Type}", GameState.Buildings);
	///   </code>
	/// </example>
	public static class JsonPrefab
	{
		/// <summary>
		///     <para>fafas</para>
		/// </summary>
		private static JsonSerializer serializer = JsonSerializer.CreateDefault();

		/// <summary>Instantiate <see cref="MonoBehaviour" />s based on a list of objects and populate them.</summary>
		/// <param name="stateObjects">List of objects to instantiate <see cref="MonoBehaviour" />s of.</param>
		/// <param name="saveOnDestroy">Should it save back the state when a <see cref="MonoBehaviour" /> is destroyed?</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" />s to instantiate.</typeparam>
		/// <returns>List of <see cref="MonoBehaviour" />s instantiated.</returns>
		public static IEnumerable<T> Instantiate<T>(IEnumerable stateObjects, bool saveOnDestroy = true) where T: MonoBehaviour
		{
			foreach (object stateObject in stateObjects)
			{
				T instance = Instantiate<T>(stateObject, saveOnDestroy);
				if (instance != null)
					yield return instance;
			}
		}

		/// <summary>Instantiate <see cref="MonoBehaviour" />s based on a list of <see cref="JObject" />s and populate them.</summary>
		/// <param name="path">Path to the prefab(s). Anything enclosed in {} gets replaced with the value of a property.</param>
		/// <param name="jObjects">List of <see cref="JObject" />s to instantiate instances of.</param>
		/// <param name="saveOnDestroy">Should it save back the state when a <see cref="MonoBehaviour" /> is destroyed?</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" />s to instantiate.</typeparam>
		/// <returns>List of <see cref="MonoBehaviour" />s instantiated.</returns>
		public static IEnumerable<T> Instantiate<T>(string path, IEnumerable<JObject> jObjects, bool saveOnDestroy = true)
			where T: MonoBehaviour
		{
			foreach (JObject jObject in jObjects)
			{
				T instance = Instantiate<T>(path, jObject, saveOnDestroy);
				if (instance != null)
					yield return instance;
			}
		}

		/// <summary>Instantiate a <see cref="MonoBehaviour" /> based on an object and populates it.</summary>
		/// <param name="stateObject">The object to instantiate a <see cref="MonoBehaviour" /> of.</param>
		/// <param name="saveOnDestroy">Should it save back the state when the <see cref="MonoBehaviour" /> is destroyed?</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" /> to instantiate.</typeparam>
		/// <returns><see cref="MonoBehaviour" /> instance instantiated, or <see langword="null" />.</returns>
		public static T Instantiate<T>(object stateObject, bool saveOnDestroy = true) where T: MonoBehaviour
		{
			Type type = stateObject.GetType();
			JsonPrefabAttribute attribute = type.GetCustomAttributes(typeof(JsonPrefabAttribute), true)
												.Cast<JsonPrefabAttribute>()
												.FirstOrDefault();
			if (attribute == null)
				return null;

			JObject jObject = JObject.FromObject(stateObject);
			T instance = InstantiateInternal<T>(attribute.Path, jObject);

			if (instance == null || !saveOnDestroy)
				return instance;

			JsonSaveStateOnDestroy serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveStateOnDestroy>();
			serializeOnDestroy.MonoObject = instance;
			serializeOnDestroy.StateObject = stateObject;

			return instance;
		}

		/// <summary>Instantiate a <see cref="MonoBehaviour" /> based on an <see cref="JObject" /> and populates it.</summary>
		/// <param name="path">Path to the prefab. Anything enclosed in {} gets replaced with the value of a property.</param>
		/// <param name="jObject">The <see cref="JObject" /> to instantiate a <see cref="MonoBehaviour" /> of.</param>
		/// <param name="saveOnDestroy">Should it save back the state when the <see cref="MonoBehaviour" /> is destroyed?</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" /> to instantiate.</typeparam>
		/// <returns><see cref="MonoBehaviour" /> instance instantiated, or <see langword="null" />.</returns>
		public static T Instantiate<T>(string path, JObject jObject, bool saveOnDestroy = true) where T: MonoBehaviour
		{
			T instance = InstantiateInternal<T>(path, jObject);
			if (instance == null || !saveOnDestroy)
				return instance;

			JsonSaveJObjectOnDestroy serializeOnDestroy = instance.gameObject.AddComponent<JsonSaveJObjectOnDestroy>();
			serializeOnDestroy.MonoObject = instance;
			serializeOnDestroy.JObject = jObject;
			return instance;
		}

		private static T InstantiateInternal<T>(string path, JObject jObject) where T: MonoBehaviour
		{
			string currentPath = ReplaceValues(path, jObject);

			T prefab = ResourceManager.Load<T>(ResourceFolder.Resources, currentPath);
			if (prefab == null)
				return null;

			T instance = Object.Instantiate(prefab);
			instance.name = prefab.name;
			using (JsonReader jObjectReader = jObject.CreateReader())
				serializer.Populate(jObjectReader, instance);

			return instance;
		}

		/// <summary>Save the current values of a list of <see cref="MonoBehaviour" />s back to their states.</summary>
		/// <param name="monoObjects">The list of <see cref="MonoBehaviour" />s to save values of.</param>
		/// <param name="stateObjects">The list of objects to save values to.</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" />s to save.</typeparam>
		public static void Save<T>(List<T> monoObjects, List<object> stateObjects) where T: MonoBehaviour
		{
			if (monoObjects.Count != stateObjects.Count)
				return;

			for (int i = 0; i < monoObjects.Count; i++)
				Save(monoObjects[i], stateObjects[i]);
		}

		/// <summary>Save the current values of a list of <see cref="MonoBehaviour" />s back to their <see cref="JObject" />s.</summary>
		/// <param name="monoObjects">The list of <see cref="MonoBehaviour" />s to save values of.</param>
		/// <param name="jObjects">The list of <see cref="JObject" />s to save values to.</param>
		/// <typeparam name="T">Type of <see cref="MonoBehaviour" />s to save.</typeparam>
		public static void Save<T>(List<T> monoObjects, List<JObject> jObjects) where T: MonoBehaviour
		{
			if (monoObjects.Count != jObjects.Count)
				return;

			for (int i = 0; i < monoObjects.Count; i++)
				Save(monoObjects[i], jObjects[i]);
		}

		/// <summary>Save the current values of a <see cref="MonoBehaviour" /> back to its state.</summary>
		/// <param name="monoObject">The <see cref="MonoBehaviour" /> to save values of.</param>
		/// <param name="stateObject">The object to save values to.</param>
		public static void Save(MonoBehaviour monoObject, object stateObject)
		{
			JObject jObject = JObject.FromObject(monoObject);
			using (JsonReader jObjectReader = jObject.CreateReader())
				serializer.Populate(jObjectReader, stateObject);
		}

		/// <summary>Save the current values of a <see cref="MonoBehaviour" /> back to its <see cref="JObject" />.</summary>
		/// <param name="monoObject">The <see cref="MonoBehaviour" /> to save values of.</param>
		/// <param name="jObject">The <see cref="JObject" /> to save values to.</param>
		public static void Save(MonoBehaviour monoObject, JObject jObject)
		{
			JObject newJObject = JObject.FromObject(monoObject);
			jObject.ReplaceAll(newJObject.Children());
		}

		/// <summary>Replace the names enclosed in {} with respective values from a <see cref="JObject" />.</summary>
		/// <param name="path">The string to replace values in.</param>
		/// <param name="jObject">The <see cref="JObject" /> to get values from.</param>
		/// <returns>A string with replaced names.</returns>
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
						startIndex = braceEndIndex                     + 1;
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

	/// <summary><see cref="JsonConverter" /> to use for the Mono-only mode.</summary>
	public class JsonPrefabConverter: JsonConverter
	{
		public readonly string Path;

		/// <summary>Create a new <see cref="JsonConverter" /> for prefab(s).</summary>
		/// <param name="path">Prefab path. Anything enclosed in {} is replaced with the value of a property.</param>
		public JsonPrefabConverter(string path)
		{
			Path = path;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken jObject = JToken.ReadFrom(reader);
			string path = JsonPrefab.ReplaceValues(Path, jObject);

			Object prefab = Resources.Load(path, objectType);
			if (prefab == null)
				return null;

			Object instance = Object.Instantiate(prefab);
			instance.name = prefab.name;
			using (JsonReader jObjectReader = jObject.CreateReader())
				serializer.Populate(jObjectReader, instance);

			return instance;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(MonoBehaviour).IsAssignableFrom(objectType);
		}

		public override bool CanWrite => false;
	}

	/// <summary>Attribute to use for the State-Mono mode.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class JsonPrefabAttribute: Attribute
	{
		public readonly string Path;

		public JsonPrefabAttribute(string path)
		{
			Path = path;
		}
	}

	/// <summary>Component added to instances created in State-Mono mode. Saves state on destruction.</summary>
	public class JsonSaveStateOnDestroy: MonoBehaviour
	{
		public MonoBehaviour MonoObject;
		public object StateObject;

		protected void OnDestroy()
		{
			JsonPrefab.Save(MonoObject, StateObject);
		}
	}

	/// <summary>Component added to instances created in JObject-Mono mode. Saves state on destruction.</summary>
	public class JsonSaveJObjectOnDestroy: MonoBehaviour
	{
		public MonoBehaviour MonoObject;
		public JObject JObject;

		protected void OnDestroy()
		{
			JsonPrefab.Save(MonoObject, JObject);
		}
	}
}