using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Weapons
{
	public static class WeaponSystem
	{
		public static Random Random = new Random();
		public const float RaycastDistance = 1.0f;
		public static readonly ContactFilter2D ContactFilter = new ContactFilter2D().NoFilter();

		static WeaponSystem()
		{
			Random.InitState();
		}
	}
}