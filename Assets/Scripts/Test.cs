using DG.Tweening;
using Engine;
using UnityEngine;

namespace Game
{
	public class Test : MonoBehaviour
	{
		public Transform Transform;

		private async void Awake()
		{
			await Transform.DOScale(Vector3.zero, 3.0f).ToUniTask();
			print("agam");

			//ModManager.LoadMods();
		}
		
		public void Button()
		{
			//UIManager.Show(Windows.Mods);
		}
	}
}