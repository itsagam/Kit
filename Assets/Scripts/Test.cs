using UniRx.Async;
using UnityEngine;
using Weapons;

namespace Game
{
	public class Test: MonoBehaviour
	{
		public Weapon Weapon;

		private void Awake()
		{
			KeepFiring().Forget();
		}

		protected async UniTask KeepFiring()
		{
			Transform transform1 = Weapon.transform;
			while (true)
			{
				if (Weapon == null)
					break;

				Weapon.Fire(transform1.position, transform1.rotation);
				await UniTask.Delay(1000);
			}
		}
	}
}