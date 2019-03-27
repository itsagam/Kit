using UniRx.Async;
using UnityEngine;
using Weapons;

namespace Game
{
	public class Test: MonoBehaviour
	{
		public Weapon Weapon;

		protected void Start()
		{
			Weapon.Fire();
			//KeepFiring().Forget();
		}

		protected async UniTask KeepFiring()
		{
			while (true)
			{
				if (Weapon == null)
					break;

				Weapon.Fire();
				await UniTask.Delay(1000);
			}
		}
	}
}