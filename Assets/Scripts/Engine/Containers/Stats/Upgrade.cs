using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Engine.Containers
{
	/// <summary>
	/// Any entity (POCO or <see cref="Sirenix.OdinInspector.SerializedMonoBehaviour"/>) that wishes to have stats should implement this
	/// interface and provide a list of Upgrades.
	/// </summary>
	public interface IUpgradeable
	{
		ReactiveCollection<Upgrade> GetUpgrades();
	}

	/// <summary>
	/// A more inspector-friendly list for use in <see cref="Sirenix.OdinInspector.SerializedMonoBehaviour"/>s.
	/// </summary>
	public class UpgradeList: ReactiveCollection<Upgrade>
	{
	}

	/// <summary>
	/// A list of effects to be identified by an ID.
	/// </summary>
	[Serializable]
	public class Upgrade
	{
		/// <summary>
		/// ID of the upgrade.
		/// </summary>
		public string ID;

		/// <summary>
		/// List of effects this upgrades causes.
		/// </summary>
		public List<Effect> Effects = new List<Effect>();

		public Upgrade()
		{
		}

		public Upgrade(string id)
		{
			ID = id;
		}

		public Upgrade(string id, IEnumerable<Effect> effects): this(id)
		{
			AddEffects(effects);
		}

		/// <summary>
		/// Add specified <see cref="Effect"/>s.
		/// </summary>
		public void AddEffects(IEnumerable<Effect> effects)
		{
			Effects.AddRange(effects);
		}

		/// <summary>
		/// Add the specified <see cref="Effect"/>.
		/// </summary>
		public void AddEffect(Effect effect)
		{
			Effects.Add(effect);
		}

		/// <summary>
		/// Create a new <see cref="Effect"/> and add it.
		/// </summary>
		/// <param name="stat">The stat ID.</param>
		/// <param name="value">The effect Type and Value represented as a string.</param>
		public void AddEffect(string stat, string value)
		{
			Effects.Add(new Effect(stat, value));
		}

		/// <summary>
		/// Create a new <see cref="Effect"/> and add it.
		/// </summary>
		/// <param name="stat">The stat ID.</param>
		/// <param name="type">The effect's Type.</param>
		/// <param name="value">The effect's Value.</param>
		public void AddEffect(string stat, EffectType type, float value)
		{
			Effects.Add(new Effect(stat, type, value));
		}

		/// <summary>
		/// Remove the specified <see cref="Effect"/>.
		/// </summary>
		public void RemoveEffect(Effect effect)
		{
			Effects.Remove(effect);
		}

		/// <summary>
		/// Remove all <see cref="Effect"/>s associated with the given stat ID.
		/// </summary>
		public void RemoveEffects(string stat)
		{
			Effects.RemoveAll(e => e.Stat == stat);
		}

		/// <summary>
		/// Find an <see cref="Upgrade" /> with its ID.
		/// </summary>
		public static Upgrade Find(IUpgradeable upgradeable, string id)
		{
			return upgradeable.GetUpgrades().FirstOrDefault(b => b.ID == id);
		}

		/// <summary>
		/// Remove an <see cref="Upgrade" /> with its ID.
		/// </summary>
		public static bool RemoveFrom(IUpgradeable upgradeable, string id)
		{
			Upgrade previous = Find(upgradeable, id);
			if (previous == null)
				return false;

			upgradeable.GetUpgrades().Remove(previous);
			return true;
		}
	}
}