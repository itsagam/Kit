using System;
using UniRx;

namespace Engine.Containers
{
	/// <summary>
	/// <para> Represents the base and current value of a single stat. </para>
	/// <para>Multiple instances of this class be used instead of the Stats class if access through individual variables is desired.</para>
	/// </summary>
	public class Stat: IDisposable
	{
		/// <summary>
		/// The <see cref="IUpgradeable"/> to use for calculating current value.
		/// </summary>
		public IUpgradeable Upgradeable;

		/// <summary>
		/// The ID of the stat.
		/// </summary>
		public string ID;

		/// <summary>
		/// The base value property of the stat.
		/// </summary>
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

		/// <summary>
		/// The current value property of the stat.
		/// </summary>
		public ReadOnlyReactiveProperty<float> Current =>
			current ?? (current = Stats.CreateCurrentProperty(Base, Upgradeable, ID));

		/// <summary>
		/// The base value of the stat.
		/// </summary>
		public float BaseValue
		{
			get => Base.Value;
			set => Base.Value = value;
		}

		/// <summary>
		/// The current value of the stat.
		/// </summary>
		public float CurrentValue => Current.Value;

		public void Dispose()
		{
			Base.Dispose();
			Current?.Dispose();
		}
	}
}