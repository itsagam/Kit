using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Toolkit;
using Newtonsoft.Json;

[JsonConverter(typeof(JsonPrefabConverter), "Enemies/{Type}")]
[JsonObject(MemberSerialization.OptIn)]
public class Enemy: MonoBehaviour
{
	public float MoveSpeed = 10;

	[JsonProperty]
	public Vector3 Position
	{
		get
		{
			return transform.position;
		}
		set
		{
			transform.position = value;
		}
	}
}