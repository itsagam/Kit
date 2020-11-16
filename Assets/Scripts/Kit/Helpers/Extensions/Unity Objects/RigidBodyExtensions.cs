using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Rigidbody" />/<see cref="Rigidbody2D" /> extensions.</summary>
	public static class RigidBodyExtensions
	{
		/// <summary>Remove the forces on the rigid-body.</summary>
		public static void Stop(this Rigidbody body)
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}

		/// <summary>Remove the forces on the rigid-body.</summary>
		public static void Stop(this Rigidbody2D body)
		{
			body.velocity = Vector2.zero;
			body.angularVelocity = 0;
		}
	}
}