using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kit.Containers
{
	/// <summary>Represents the action to take when another buff with the same ID already exists.</summary>
	public enum BuffMode
	{
		/// <summary>Do nothing. Use both.</summary>
		Nothing,

		/// <summary>Extend the duration of the previous buff instead.</summary>
		Extend,

		/// <summary>Keep the previous buff and discard the new one.</summary>
		Keep,

		/// <summary>Set the duration of the previous buff equal to the duration of the new one.</summary>
		Replace,

		/// <summary>Keep the longer of the two buffs.</summary>
		Longer,

		/// <summary>Keep the shorter of the two buffs.</summary>
		Shorter
	}

	/// <summary>
	///     <para>Represents an <see cref="Upgrade" /> that is only applicable for a specified duration, and gets removed afterwards.</para>
	///     <para>Needs to be added through <see cref="Apply(IUpgradeable)" /> to an <see cref="IUpgradeable" /> for the timer to start.</para>
	/// </summary>
	/// <example>
	///     <code>
	/// float buffTime = 10.0f;
	/// Buff damageBuff = new Buff("DamagePickup", new [] { new Effect("Damage", "x2") }, buffTime);
	/// damageBuff.AddTo(ship);
	/// </code>
	/// </example>
	[Serializable]
	public class Buff: Upgrade
	{
		/// <summary>Duration of the buff in seconds.</summary>
		public float Duration;

		/// <summary>Action to take when a buff with the same ID already exists.</summary>
		public BuffMode Mode = BuffMode.Extend;

		/// <summary>Time remaining in seconds before the buff expires.</summary>
		public float TimeLeft { get; protected set; } = -1;

		/// <summary>Create a new Buff.</summary>
		public Buff()
		{
		}

		/// <summary>Create a new Buff.</summary>
		/// <param name="id">Upgrade ID.</param>
		/// <param name="duration">Duration of the buff in seconds.</param>
		/// <param name="mode">The <see cref="BuffMode" /> to use.</param>
		public Buff(string id, float duration, BuffMode mode = BuffMode.Extend)
		{
			ID = id;
			Duration = duration;
			Mode = mode;
		}

		/// <summary>Create a new Buff.</summary>
		/// <param name="id">Upgrade ID.</param>
		/// <param name="effects">List of Upgrade effects.</param>
		/// <param name="duration">Duration of the Buff in seconds.</param>
		/// <param name="mode">The <see cref="BuffMode" /> to use.</param>
		public Buff(string id, IEnumerable<Effect> effects, float duration, BuffMode mode = BuffMode.Extend)
			: this(id, duration, mode)
		{
			AddEffects(effects);
		}

		/// <summary>Add the buff to an <see cref="IUpgradeable" /> and start the timer.</summary>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> to apply the buff on.</param>
		public virtual Buff Apply(IUpgradeable upgradeable)
		{
			return Apply(upgradeable, Mode);
		}

		/// <inheritdoc cref="Apply(IUpgradeable)"/>
		/// <param name="mode"><see cref="BuffMode" /> override.</param>
		public virtual Buff Apply(IUpgradeable upgradeable, BuffMode mode)
		{
			Buff previous = null;
			if (mode != BuffMode.Nothing)
				previous = Find(upgradeable, ID) as Buff;

			if (mode == BuffMode.Nothing || previous == null)
			{
				upgradeable.GetUpgrades().Add(this);
				StartTimer(upgradeable).Forget();
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

		protected virtual async UniTaskVoid StartTimer(IUpgradeable upgradeable)
		{
			float end = Time.time + Duration;
			while ((TimeLeft = end - Time.time) > 0)
				await UniTask.Yield();
			try
			{
				upgradeable.GetUpgrades().Remove(this);
			}
			catch
			{
			}
		}

		/// <summary>Remove the buff from an <see cref="IUpgradeable" />.</summary>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> which contains the buff.</param>
		/// <returns>Whether the buff was successfully removed.</returns>
		public virtual bool RemoveFrom(IUpgradeable upgradeable)
		{
			return upgradeable.GetUpgrades().Remove(this);
		}
	}
}