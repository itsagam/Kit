using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game
{
	public class GameState
	{
		public List<JObject> Enemies;
	}
	
	[JsonPrefab("Enemies/{Type}")]
	public class EnemyState
	{
		public string Type;
		public Vector3 Position;
	}
}