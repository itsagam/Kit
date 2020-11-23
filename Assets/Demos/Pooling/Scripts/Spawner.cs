using UnityEngine;

namespace Demos.Pooling
{
	public class Spawner: MonoBehaviour
	{
		public Transform Prefab;
		public int Rows = 3;
		public int Columns = 10;
		public Vector2 Spacing = new Vector2(1f, 1f);

		protected new Transform transform;

		protected void Awake()
		{
			transform = base.transform;

			SpawnAll();
		}

		protected void SpawnAll()
		{
			float half = Columns / 2.0f;
			for (int column = 0; column < Columns; column++)
				for (int row = 0; row < Rows; row++)
				{
					Vector3 position = new Vector3((column - half) * Spacing.x, row * Spacing.y, 0);
					position += transform.position;
					if (row % 2 == 0)
						position.x += Spacing.x / 2.0f;
					Spawn(position);
				}
		}

		public void Spawn(Vector3 position)
		{
			Transform instance = Instantiate(Prefab, position, transform.rotation);
		}
	}
}