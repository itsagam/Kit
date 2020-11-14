﻿using UnityEngine;

namespace Engine
{
	/// <summary>
	/// <see cref="Behaviour"/> extensions.
	/// </summary>
	public static class BehaviourExtensions
	{
		/// <summary>
		/// Set the enabled property to <see langword="true" />.
		/// </summary>
		public static void Enable(this Behaviour behaviour)
		{
			behaviour.enabled = true;
		}

		/// <summary>
		/// Set the enabled property to <see langword="false" />.
		/// </summary>
		public static void Disable(this Behaviour behaviour)
		{
			behaviour.enabled = false;
		}
	}
}