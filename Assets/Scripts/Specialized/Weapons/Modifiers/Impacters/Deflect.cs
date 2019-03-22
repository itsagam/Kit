using UnityEngine;

namespace Weapons.Modifiers.Impacters
{
	public class Deflect : IImpact
	{
		public bool OnImpact(Transform fireable, Transform impact, Vector3 position, Vector2 normal)
		{
			fireable.up = Vector3.Reflect(fireable.up, normal);
			return false;
		}
	}
}