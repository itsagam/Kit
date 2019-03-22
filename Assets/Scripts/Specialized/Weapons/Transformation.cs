using UnityEngine;

namespace Weapons
{
	public struct Transformation
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public Transformation(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}
	}
}
