using UnityEngine;

namespace Weapons
{
	public interface IImpact
	{
		bool OnImpact(Transform fireable, Transform impact, Vector3 position, Vector2 normal);
	}
}