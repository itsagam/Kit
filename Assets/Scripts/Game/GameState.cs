using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Game
{
	public class GameState
	{
		public List<Enemy> Enemies;
	}

	public class EnemyState
	{
		public string Type;
		public Vector3 Position;
	}
}