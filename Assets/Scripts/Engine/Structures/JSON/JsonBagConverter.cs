using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

public abstract class JsonBagConverter<T> : JsonConverter
{
    public abstract T GetObject(string key);
    public abstract string GetKey(T obj);

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Bag<T>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Bag<T> bag = (Bag<T>)value;
        Dictionary<string, int> output = new Dictionary<string, int>();
        foreach (var kvp in bag)
            output[GetKey(kvp.Key)] = kvp.Value;
        serializer.Serialize(writer, output);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);
        Bag<T> bag = new Bag<T>();
        foreach (var item in jObject)
            bag[GetObject(item.Key)] = item.Value.Value<int>();
        return bag;
    }

    public override bool CanRead
    {
        get
        {
            return true;
        }
    }

    public override bool CanWrite
    {
        get
        {
            return true;
        }
    }
}