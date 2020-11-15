using UnityEngine;

namespace Kit
{
	/// <summary>
	/// <see cref="Behaviour"/> extensions.
	/// </summary>
	public static class BehaviourExtensions
	{
		/// <summary>
		/// Set the <see cref="Behaviour.enabled"/> property to <see langword="true" />.
		/// </summary>
		public static void Enable(this Behaviour behaviour)
		{
			behaviour.enabled = true;
		}

		/// <summary>
		/// Set the <see cref="Behaviour.enabled"/> property to <see langword="false" />.
		/// </summary>
		public static void Disable(this Behaviour behaviour)
		{
			behaviour.enabled = false;
		}
	}
}