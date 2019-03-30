using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Weapons
{
	public static class WeaponSystem
	{
		public static readonly float3 Right = new float3(1.0f, 0.0f, 0.0f);
		public static readonly float3 Up = new float3(0.0f, 1.0f, 0.0f);
		public static readonly float3 Forward = new float3(0.0f, 0.0f, 1.0f);

		public static Random Random = new Random();

		public const float RaycastDistance = 1.0f;
		public static readonly ContactFilter2D ContactFilter = new ContactFilter2D().NoFilter();

		static WeaponSystem()
		{
			Random.InitState();
		}
	}
}