using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Kit.Containers
{
	/// <summary>
	/// Represents the action to take when another buff with the same ID already exists.
	/// </summary>
	public enum BuffMode
	{
		/// <summary>
		/// Do nothing. Use both.
		/// </summary>
		Nothing,

		/// <summary>
		/// Extend the duration of the previous buff instead.
		/// </summary>
		Extend,

		/// <summary>
		/// Keep the previous buff and discard the new one.
		/// </summary>
		Keep,

		/// <summary>
		/// Set the duration of the previous buff equal to the duration of the new one.
		/// </summary>
		Replace,

		/// <summary>
		/// Keep the longer of the two buffs.
		/// </summary>
		Longer,

		/// <summary>
		/// Keep the shorter of the two buffs.
		/// </summary>
		Shorter
	}

    /// <summary>
    /// <para>Represents an <see cref="Upgrade"/> that is only applicable for a specified duration, and gets removed afterwards.</para>
    /// <para>Needs to be added through <see cref="AddTo(IUpgradeable)"/> to an <see cref="IUpgradeable" /> for the timer to start.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// float buffTime = 10.0f;
    /// Buff damageBuff = new Buff("DamagePickup", new [] { new Effect("Damage", "x2") }, buffTime);
    /// damageBuff.AddTo(ship);
    /// </code>
    /// </example>
    [Serializable]
	public class Buff: Upgrade
	{
		/// <summary>
		/// Duration of the buff in seconds.
		/// </summary>
		public float Duration;

		/// <summary>
		/// Action to take when a buff with the same ID already exists.
		/// </summary>
		public BuffMode Mode = BuffMode.Extend;

		/// <summary>
		/// Time remaining in seconds before the buff expires.
		/// </summary>
		public ReactiveProperty<float> TimeLeft { get; } = new ReactiveProperty<float>(-1);


		/// <summary>
		/// Create a new Buff.
		/// </summary>
		public Buff()
		{
		}

		/// <summary>
		/// Create a new Buff.
		/// </summary>
		/// <param name="id">Upgrade ID.</param>
		/// <param name="duration">Duration of the buff in seconds.</param>
		/// <param name="mode">The BuffMode to use.</param>
		public Buff(string id, float duration, BuffMode mode = BuffMode.Extend)
		{
			ID = id;
			Duration = duration;
			Mode = mode;
		}

		/// <summary>
		/// Create a new Buff.
		/// </summary>
		/// <param name="id">Upgrade ID.</param>
		/// <param name="effects">List of Upgrade effects.</param>
		/// <param name="duration">Duration of the Buff in seconds.</param>
		/// <param name="mode">The BuffMode to use.</param>
		public Buff(string id, IEnumerable<Effect> effects, float duration, BuffMode mode = BuffMode.Extend)
			: this(id, duration, mode)
		{
			AddEffects(effects);
		}

		/// <summary>
		/// Add the Buff to an IUpgradeable and start the timer.
		/// </summary>
		/// <param name="upgradeable">The IUpgradeable to add the buff to.</param>
		public virtual Buff AddTo(IUpgradeable upgradeable)
		{
			return AddTo(upgradeable, Mode);
		}

		/// <summary>
		/// Add the Buff to an IUpgradeable and start the timer.
		/// </summary>
		/// <param name="upgradeable">The IUpgradeable to add the buff to.</param>
		/// <param name="mode">BuffMode override.</param>
		public virtual Buff AddTo(IUpgradeable upgradeable, BuffMode mode)
		{
			Buff previous = null;
			if (mode != BuffMode.Nothing)
				previous = Find(upgradeable, ID) as Buff;

			if (mode == BuffMode.Nothing || previous == null)
			{
				upgradeable.GetUpgrades().Add(this);
				float end = Time.time + Duration;
				Observable.EveryUpdate()
						  .Select(l => end - Time.time)
						  .TakeWhile(t => t > 0)
						  .Subscribe(time => TimeLeft.Value = time,
									 () =>
									 {
										 try
										 {
											 upgradeable.GetUpgrades().Remove(this);
										 }
										 catch
										 {
										 }
									 });
				return this;
			}

			switch (mode)
			{
				case BuffMode.Keep:
					break;

				case BuffMode.Replace:
					previous.Duration = Duration;
					break;

				case BuffMode.Extend:
					previous.Duration += Duration;
					break;

				case BuffMode.Longer:
					if (previous.Duration < Duration)
						previous.Duration = Duration;
					break;

				case BuffMode.Shorter:
					if (previous.Duration > Duration)
						previous.Duration = Duration;
					break;
			}

			return this;
		}

		/// <summary>
		/// Remove the Buff from an IUpgradeable.
		/// </summary>
		/// <param name="upgradeable">The IUpgradeable to remove the buff from.</param>
		/// <returns>Whether the item was successfully removed.</returns>
		public virtual bool RemoveFrom(IUpgradeable upgradeable)
		{
			return upgradeable.GetUpgrades().Remove(this);
		}
	}
}