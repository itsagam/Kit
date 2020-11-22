using Kit.Behaviours;
using UnityEngine;

namespace Demos.Pooling
{
	public class Projectile: PoolWithTime
	{
		public override void AwakeFromPool()
		{
			base.AwakeFromPool();

			// We have to reset rotation since Fire2 alters it
			transform.rotation = Quaternion.identity;
		}
	}
}