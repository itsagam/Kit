using System;
using Engine.Parsers;
using UnityEngine;

namespace Engine
{
	public static class PreferenceManager
	{
		public static void Set(string category, string entity, string property, object value)
		{
			string key = GetKey(category, entity, property);

			switch (value)
			{
				case int @int:
					PlayerPrefs.SetInt(key, @int);
					break;

				case uint @uint:
					PlayerPrefs.SetInt(key, (int) @uint);
					break;

				case float @float:
					PlayerPrefs.SetFloat(key, @float);
					break;

				case string @string:
					PlayerPrefs.SetString(key, @string);
					break;

				case bool @bool:
					PlayerPrefs.SetInt(key, @bool ? 1 : 0);
					break;

				case byte @byte:
					PlayerPrefs.SetInt(key, @byte);
					break;

				case sbyte @sbyte:
					PlayerPrefs.SetInt(key, @sbyte);
					break;

				case char @char:
					PlayerPrefs.SetInt(key, @char);
					break;

				case short @short:
					PlayerPrefs.SetInt(key, @short);
					break;
				case ushort @ushort:
					PlayerPrefs.SetInt(key, @ushort);
					break;

				case Enum @enum:
					PlayerPrefs.SetInt(key, Convert.ToInt32(@enum));
					break;

				default:
					PlayerPrefs.SetString(key, JSONParser.ToJson(value));
					break;
			}
		}

		public static T Get<T>(string category, string entity, string property, T defaultValue)
		{
			string key = GetKey(category, entity, property);
			switch (defaultValue)
			{
				case int @int:
					return (T) (object) PlayerPrefs.GetInt(key, @int);

				case uint @uint:
					return (T) (object) PlayerPrefs.GetInt(key, (int) @uint);

				case float @float:
					return (T) (object) PlayerPrefs.GetFloat(key, @float);

				case string @string:
					return (T) (object) PlayerPrefs.GetString(key, @string);

				case bool @bool:
					return (T) (object) (PlayerPrefs.GetInt(key, @bool ? 1 : 0) == 1);

				case byte @byte:
					return (T) (object) PlayerPrefs.GetInt(key, @byte);

				case sbyte @sbyte:
					return (T) (object) PlayerPrefs.GetInt(key, @sbyte);

				case char @char:
					return (T) (object) PlayerPrefs.GetInt(key, @char);

				case short @short:
					return (T) (object) PlayerPrefs.GetInt(key, @short);

				case ushort @ushort:
					return (T) (object) PlayerPrefs.GetInt(key, @ushort);

				case Enum @enum:
					return (T) Enum.ToObject(typeof(T), PlayerPrefs.GetInt(key, Convert.ToInt32(@enum)));

				default:
					if (!PlayerPrefs.HasKey(key))
						return defaultValue;

					string serialized = PlayerPrefs.GetString(key, null);
					T deserialized = JSONParser.FromJson<T>(serialized);
					return deserialized;
			}
		}

		public static void Delete(string category, string entity, string property)
		{
			PlayerPrefs.DeleteKey(GetKey(category, entity, property));
		}

		public static bool Exists(string category, string entity, string property)
		{
			return PlayerPrefs.HasKey(GetKey(category, entity, property));
		}

		private static string GetKey(string category, string entity, string property)
		{
			return $"{category}/{entity}.{property}";
		}

		public static void Clear()
		{
			PlayerPrefs.DeleteAll();
		}

		public static void Save()
		{
			PlayerPrefs.Save();
		}
	}
}