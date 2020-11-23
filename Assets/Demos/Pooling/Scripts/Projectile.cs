using Kit.Behaviours;
using Kit.Pooling;
using UnityEngine;

namespace Demos.Pooling
{
	public class Projectile: PoolWithTime
	{
		public float Damage = 25.0f;

		protected float spawnTime;

		public override void AwakeFromPool()
		{
			base.AwakeFromPool();

			spawnTime = UnityEngine.Time.time;

			// We have to reset rotation since Fire2 alters it
			transform.rotation = Quaternion.identity;
		}

		protected void OnTriggerEnter2D(Collider2D other)
		{
			// Avoid self-collision checks
			if (UnityEngine.Time.time - spawnTime < 0.05f)
				return;

			Ship ship = other.GetComponent<Ship>();
			if (ship != null)
			{
				ship.Hit(Damage);
				Pooler.Destroy(this);
			}
		}
	}
}