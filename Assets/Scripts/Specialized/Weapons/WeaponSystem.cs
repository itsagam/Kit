using Unity.Mathematics;

namespace Weapons
{
	public static class WeaponSystem
	{
		public static Random Random = new Random();

		static WeaponSystem()
		{
			Random.InitState();
		}
	}
}