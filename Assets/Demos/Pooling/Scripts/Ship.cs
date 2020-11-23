using Kit;
using UnityEngine;

namespace Demos.Pooling
{
	public class Ship: MonoBehaviour
	{
		public float Health = 100.0f;
		public AudioClip HitSound;
		public AudioClip DeathSound;
		public ParticleSystem HitEffect;
		public ParticleSystem DeathEffect;

		public void Hit(float damage)
		{
			Health = Mathf.Max(0, Health - damage);
			if (Health <= 0)
				Die();

			AudioManager.PlaySound(HitSound);
			EffectsManager.Spawn(HitEffect, transform.position);
		}

		public void Die()
		{
			EffectsManager.Spawn(DeathEffect, transform.position);
			AudioManager.Play(DeathSound);
			gameObject.Destroy();
		}
	}
}