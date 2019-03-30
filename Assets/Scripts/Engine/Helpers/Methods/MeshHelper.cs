using UnityEngine;

namespace Engine
{
	public static class MeshHelper
	{
		public static Mesh GenerateQuad(float size, Vector2 pivot)
		{
			Vector3[] vertices =
			{
				new Vector3(size - pivot.x, size - pivot.y, 0),
				new Vector3(size - pivot.x, 0    - pivot.y, 0),
				new Vector3(0    - pivot.x, 0    - pivot.y, 0),
				new Vector3(0    - pivot.x, size - pivot.y, 0)
			};

			Vector2[] uv =
			{
				new Vector2(1, 1),
				new Vector2(1, 0),
				new Vector2(0, 0),
				new Vector2(0, 1)
			};

			int[] triangles =
			{
				0, 1, 2,
				2, 3, 0
			};

			return new Mesh
				   {
					   vertices = vertices,
					   uv = uv,
					   triangles = triangles
				   };
		}
	}
}