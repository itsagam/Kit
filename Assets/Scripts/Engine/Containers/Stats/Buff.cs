using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Engine.Containers
{
	public enum BuffMode
	{
		Nothing,
		Extend,
		Keep,
		Replace,
		Longer,
		Shorter,
	}

	[Serializable]
	public class Buff : Upgrade
	{
		public float Duration;
		public BuffMode Mode = BuffMode.Extend;

		public ReactiveProperty<float> TimeLeft { get; } = new ReactiveProperty<float>(-1);

		public Buff()
		{
		}

		public Buff(string id, float time, BuffMode mode = BuffMode.Extend)
		{
			ID = id;
			Duration = time;
			Mode = mode;
		}

		public Buff(string id, IEnumerable<Effect> effects, float time, BuffMode mode = BuffMode.Extend)
			: this(id, time, mode)
		{
			AddEffects(effects);
		}

		public virtual Buff AddTo(IUpgradeable upgradeable)
		{
			return AddTo(upgradeable, Mode);
		}

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

		public virtual bool RemoveFrom(IUpgradeable upgradeable)
		{
			return upgradeable.GetUpgrades().Remove(this);
		}
	}
}