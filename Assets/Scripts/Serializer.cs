using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Serializer : MonoBehaviour
{
	public JToken Token;
	public object Object;

	public void Serialize()
	{
		JToken newToken = JObject.FromObject(Object);
		Token.Replace(newToken);
		Token = newToken;
	}

	protected void OnDestroy()
	{
		Serialize();
	}
}