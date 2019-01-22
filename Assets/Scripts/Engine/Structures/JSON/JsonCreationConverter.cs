using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

public abstract class JsonCreationConverter<T> : JsonConverter
{
    protected abstract T Create(JObject jObject);
    protected virtual T Create(string className)
    {
        Type type = typeof(T);
        return (T)type.Assembly.CreateInstance(type.Namespace + "." + className);
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, 
                                 object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);
        object target = Create(jObject);
        serializer.Populate(jObject.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
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
            return false;
        }
    }
}