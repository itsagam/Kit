using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Engine.Containers
{
	// To create nested inventories, create Bag<object> and add other bags
	[Serializable]
	public class Bag<T> : Dictionary<T, int>
	{
		public event Action<T, int> Added;
		public event Action<T, int> Changed;
		public event Action<T, int> Removed;

		public IDictionary<T, int> Max;

		// Every call ultimately reaches here
		public new int this [T item]
		{
			get => TryGetValue(item, out int value) ? value : 0;
			set
			{
				int clamped = Max != null && Max.TryGetValue(item, out int max) ? Mathf.Min(value, max) : value;
				if (clamped > 0)
				{
					bool contained = ContainsKey(item);
					base[item] = clamped;
					if (contained)
						Changed?.Invoke(item, clamped);
					else
						Added?.Invoke(item, clamped);
				}
				else
				{
					if (base.Remove(item))
						Removed?.Invoke(item, 0);
				}
			}
		}

		public new void Add(T item, int amount = 1)
		{
			this[item] += amount;
		}

		public void Add(KeyValuePair<T, int> kvp)
		{
			Add(kvp.Key, kvp.Value);
		}

		public void Add(Bunch<T> bunch)
		{
			Add(bunch.Item, bunch.Amount);
		}

		public void Add(IDictionary<T, int> bag)
		{
			foreach (var field in bag)
				Add(field);
		}

		public bool Remove(T item, int amount = 1)
		{
			if (!Contains(item, amount))
				return false;

			this[item] -= amount;

			return true;
		}

		public bool RemoveAll(T item)
		{
			if (!ContainsKey(item))
				return false;

			this[item] = 0;

			return true;
		}

		public bool RemoveAll(IEnumerable<T> items)
		{
			bool success = true;
			foreach (T item in items)
				if (ContainsKey(item))
					this[item] = 0;
				else
					success = false;
			return success;
		}

		public bool Remove(KeyValuePair<T, int> kvp)
		{
			return Remove(kvp.Key, kvp.Value);
		}

		public bool Remove(Bunch<T> bunch)
		{
			return Remove(bunch.Item, bunch.Amount);
		}

		public bool Remove(IDictionary<T, int> bag)
		{
			if (!Contains(bag))
				return false;

			foreach ((T key, int value) in bag)
				this[key] -= value;

			return true;
		}

		public bool Contains(T item, int amount)
		{
			return this[item] >= amount;
		}

		public bool Contains(Bunch<T> bunch)
		{
			return Contains(bunch.Item, bunch.Amount);
		}

		public bool Contains(KeyValuePair<T, int> kvp)
		{
			return Contains(kvp.Key, kvp.Value);
		}

		public bool Contains(IDictionary<T, int> bag)
		{
			return bag.All(Contains);
		}

		public static Bag<T> operator +(Bag<T> bag, T item)
		{
			bag.Add(item);
			return bag;
		}

		public static Bag<T> operator +(Bag<T> bagTo, IDictionary<T, int> bagFrom)
		{
			bagTo.Add(bagFrom);
			return bagTo;
		}

		public static Bag<T> operator +(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Add(bunch);
			return bag;
		}

		public static Bag<T> operator +(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Add(kvp);
			return bag;
		}

		public static Bag<T> operator -(Bag<T> bag, T item)
		{
			bag.Remove(item);
			return bag;
		}

		public static Bag<T> operator -(Bag<T> bagTo, IDictionary<T, int> bagFrom)
		{
			bagTo.Remove(bagFrom);
			return bagTo;
		}

		public static Bag<T> operator -(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Remove(bunch);
			return bag;
		}

		public static Bag<T> operator -(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Remove(kvp);
			return bag;
		}

		public IEnumerable<Bunch<T>> AsBunches()
		{
			return this.Select(kvp => new Bunch<T>(kvp));
		}

		public List<Bunch<T>> ToBunches()
		{
			return AsBunches().ToList();
		}
	}

	// Bunch<T> is just KeyValuePair<T, int> with operators (would've just derived from KeyValuePair but that's struct)
	[Serializable]
	public struct Bunch<T>
	{
		public T Item;
		public int Amount;

		public Bunch(KeyValuePair<T, int> pair) : this(pair.Key, pair.Value)
		{
		}

		public Bunch(Bunch<T> field) : this(field.Item, field.Amount)
		{
		}

		public Bunch(T item, int amount = 0)
		{
			Item = item;
			Amount = amount;
		}

		public static Bunch<T> operator *(Bunch<T> bunch, float multiply)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount * multiply));
		}

		public static Bunch<T> operator /(Bunch<T> bunch, float divide)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount * divide));
		}

		public static Bunch<T> operator +(Bunch<T> bunch, float plus)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount + plus));
		}

		public static Bunch<T> operator -(Bunch<T> bunch, float minus)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount - minus));
		}

		public static Bunch<T> operator *(Bunch<T> bunch, int multiply)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount * multiply);
		}

		public static Bunch<T> operator /(Bunch<T> bunch, int divide)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount * divide);
		}

		public static Bunch<T> operator +(Bunch<T> bunch, int plus)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount + plus);
		}

		public static Bunch<T> operator -(Bunch<T> bunch, int minus)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount - minus);
		}

		public static Bunch<T> operator *(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount * bunch2.Amount);
		}

		public static Bunch<T> operator /(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount / bunch2.Amount);
		}

		public static Bunch<T> operator +(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount + bunch2.Amount);
		}

		public static Bunch<T> operator -(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount - bunch2.Amount);
		}

		public KeyValuePair<T, int> ToKVP()
		{
			return new KeyValuePair<T, int>(Item, Amount);
		}
	}
}