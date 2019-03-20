using System;
using UniRx;

namespace Engine.Containers
{
	[Serializable]
	public class Stat : IDisposable
	{
		public IUpgradeable Upgradeable;
		public string ID;

		public readonly StatBaseProperty Base = new StatBaseProperty();
		protected ReadOnlyReactiveProperty<float> current;

		public Stat()
		{
		}

		public Stat(IUpgradeable upgradeable, string id)
		{
			Upgradeable = upgradeable;
			ID = id;
		}

		public ReadOnlyReactiveProperty<float> Current =>
			current ?? (current = Stats.CreateCurrentProperty(Base, Upgradeable, ID));

		public float BaseValue
		{
			get => Base.Value;
			set => Base.Value = value;
		}

		public float CurrentValue => Current.Value;

		public void Dispose()
		{
			Base.Dispose();
			Current?.Dispose();
		}
	}
}