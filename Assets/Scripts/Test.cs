using Engine.Containers;
using Sirenix.OdinInspector;
using UniRx;

namespace Game
{
	public class Test: SerializedMonoBehaviour, IUpgradeable
	{
		public Stat Health;
		public ReactiveCollection<Upgrade> Upgrades;

		private void Awake()
		{
		}

		public ReactiveCollection<Upgrade> GetUpgrades()
		{
			return Upgrades;
		}
	}
}