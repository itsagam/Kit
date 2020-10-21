using UnityEngine;

namespace Engine
{
	public static class RigidBodyExtensions
	{
		public static void Stop(this Rigidbody2D body)
		{
			body.velocity = Vector2.zero;
			body.angularVelocity = 0;
		}
	}
}