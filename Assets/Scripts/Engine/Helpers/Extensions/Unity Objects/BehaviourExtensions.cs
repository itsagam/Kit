using UnityEngine;

namespace Engine
{
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