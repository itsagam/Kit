﻿using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Marks the <see cref="GameObject"/> to be persistent across scenes.
	/// </summary>
	public class DontDestroy: MonoBehaviour
	{
		protected void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}