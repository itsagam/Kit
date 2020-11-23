using UnityEngine;

namespace Demos.Pooling
{
	public class Ship: MonoBehaviour
	{
		public float Health = 100.0f;

		public void Hit(float damage)
		{
			Health = Mathf.Max(0, Health - damage);
			if (Health <= 0)
				Die();
		}

		public void Die()
		{

		}
	}
}