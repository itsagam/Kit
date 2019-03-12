using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Engine.Containers
{
	[Serializable]
	public class Bag<T> : Dictionary<T, int>
	{
		public event Action<T, int> OnChanged;

		public Dictionary<T, int> Max;

		public new int this [T unit]
		{
			get => TryGetValue(unit, out int stored) ? stored : 0;
			set
			{
				int final = Max != null && Max.TryGetValue(unit, out int max) ? Mathf.Min(value, max) : value;
				if (final > 0)
					base[unit] = final;
				else
					Remove(unit);
				OnChanged?.Invoke(unit, final);
			}
		}

		public bool Contains(Bunch<T> bunch)
		{
			return Contains(bunch.Unit, bunch.Amount);
		}

		public bool Contains(KeyValuePair<T, int> kvp)
		{
			return Contains(kvp.Key, kvp.Value);
		}

		public bool Contains(T type, int amount)
		{
			return this[type] >= amount;
		}

		public bool Contains(Bag<T> bag)
		{
			return bag.All(Contains);
		}

		public void Store(KeyValuePair<T, int> kvp)
		{
			Store(kvp.Key, kvp.Value);
		}

		public void Store(Bunch<T> bunch)
		{
			Store(bunch.Unit, bunch.Amount);
		}

		public void Store(T type, int amount)
		{
			this[type] += amount;
		}

		public void Store(Bag<T> bag)
		{
			foreach (var field in bag)
				Store(field);
		}

		public bool Consume(T type, int amount)
		{
			if (!Contains(type, amount))
				return false;

			this[type] -= amount;

			return true;
		}

		public bool Consume(KeyValuePair<T, int> kvp)
		{
			return Consume(kvp.Key, kvp.Value);
		}

		public bool Consume(Bunch<T> bunch)
		{
			return Consume(bunch.Unit, bunch.Amount);
		}

		public bool Consume(Bag<T> bag)
		{
			if (!Contains(bag))
				return false;

			foreach (var field in bag)
				this[field.Key] -= field.Value;

			return true;
		}

		public static Bag<T> operator +(Bag<T> bagTo, Bag<T> bagFrom)
		{
			bagTo.Store(bagFrom);
			return bagTo;
		}

		public static Bag<T> operator +(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Store(bunch);
			return bag;
		}

		public static Bag<T> operator +(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Store(kvp);
			return bag;
		}

		public static Bag<T> operator -(Bag<T> bagTo, Bag<T> bagFrom)
		{
			bagTo.Consume(bagFrom);
			return bagTo;
		}

		public static Bag<T> operator -(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Consume(bunch);
			return bag;
		}

		public static Bag<T> operator -(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Consume(kvp);
			return bag;
		}

		public IEnumerable<Bunch<T>> AsEnumerable()
		{
			return this.Select(kvp => new Bunch<T>(kvp));
		}

		public List<Bunch<T>> ToList()
		{
			return AsEnumerable().ToList();
		}
	}

	[Serializable]
	public struct Bunch<T>
	{
		public T Unit;
		public int Amount;

		public Bunch(KeyValuePair<T, int> pair) : this(pair.Key, pair.Value)
		{
		}

		public Bunch(Bunch<T> field) : this(field.Unit, field.Amount)
		{
		}

		public Bunch(T unit, int amount = 0)
		{
			Unit = unit;
			Amount = amount;
		}

		public static Bunch<T> operator *(Bunch<T> bunch, float multiply)
		{
			return new Bunch<T>(bunch.Unit, (int) (bunch.Amount * multiply));
		}

		public static Bunch<T> operator /(Bunch<T> bunch, float divide)
		{
			return new Bunch<T>(bunch.Unit, (int) (bunch.Amount * divide));
		}

		public static Bunch<T> operator +(Bunch<T> bunch, float plus)
		{
			return new Bunch<T>(bunch.Unit, (int) (bunch.Amount + plus));
		}

		public static Bunch<T> operator -(Bunch<T> bunch, float minus)
		{
			return new Bunch<T>(bunch.Unit, (int) (bunch.Amount - minus));
		}

		public static Bunch<T> operator *(Bunch<T> bunch, int multiply)
		{
			return new Bunch<T>(bunch.Unit, bunch.Amount * multiply);
		}

		public static Bunch<T> operator /(Bunch<T> bunch, int divide)
		{
			return new Bunch<T>(bunch.Unit, bunch.Amount * divide);
		}

		public static Bunch<T> operator +(Bunch<T> bunch, int plus)
		{
			return new Bunch<T>(bunch.Unit, bunch.Amount + plus);
		}

		public static Bunch<T> operator -(Bunch<T> bunch, int minus)
		{
			return new Bunch<T>(bunch.Unit, bunch.Amount - minus);
		}

		public static Bunch<T> operator *(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Unit, bunch1.Amount * bunch2.Amount);
		}

		public static Bunch<T> operator /(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Unit, bunch1.Amount / bunch2.Amount);
		}

		public static Bunch<T> operator +(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Unit, bunch1.Amount + bunch2.Amount);
		}

		public static Bunch<T> operator -(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Unit, bunch1.Amount - bunch2.Amount);
		}

		public KeyValuePair<T, int> ToKVP()
		{
			return new KeyValuePair<T, int>(Unit, Amount);
		}

		public override string ToString()
		{
			return $"[{Unit}, {Amount}]";
		}
	}
}