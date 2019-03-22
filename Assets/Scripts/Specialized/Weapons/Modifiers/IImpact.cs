using UnityEngine;

namespace Weapons.Modifiers
{
	public interface IImpact
	{
		bool OnImpact(Transform fireable, Transform impact, Vector3 position, Vector2 normal);
	}
}