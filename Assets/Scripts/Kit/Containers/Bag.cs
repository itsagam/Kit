using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Containers
{
	/// <summary>
	/// A generic class that holds how much of a particular item you carry. Can be used to create things like Inventories or Wallets.
	/// </summary>
	/// <typeparam name="T">Base type of items.</typeparam>
	/// <example>
	/// <code>
	/// Bag&lt;Currency&gt; wallet = new Bag&lt;Currency&gt;();
	/// wallet.Add(Currency.Diamonds, 15);
	/// wallet -= new Bunch&lt;Currency&gt;(Currency.Diamonds, 10);
	/// </code>
	/// </example>
	/// <remarks>To create nested inventories, create <c>Bag&lt;object&gt;</c> and add other bags.</remarks>
	[DictionaryDrawerSettings(KeyLabel = "Item", ValueLabel = "Amount")]
	public class Bag<T>: Dictionary<T, int>
	{
		/// <summary>
		/// Event called whenever a new item is added.
		/// </summary>
		public event Action<T, int> Added;

		/// <summary>
		/// Event called whenever an item's amount is changed.
		/// </summary>
		public event Action<T, int> Changed;

		/// <summary>
		/// Event called whenever an item is removed.
		/// </summary>
		public event Action<T, int> Removed;

		/// <summary>
		/// A dictionary that tells the maximum amount of items of different types can be there. The amount of those types will not go
		/// beyond this number if it is specified. Ignored if <see langword="null" />.
		/// </summary>
		public IDictionary<T, int> Max;

		/// <summary>
		/// Returns the current amount of an item.
		/// </summary>
		public new int this[T item]
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

		/// <summary>
		/// Adds the amount of items.
		/// </summary>
		public new void Add(T item, int amount = 1)
		{
			this[item] += amount;
		}

		/// <summary>
		/// Adds the amount of items.
		/// </summary>
		public void Add(KeyValuePair<T, int> kvp)
		{
			Add(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Adds the amount of items.
		/// </summary>
		public void Add(Bunch<T> bunch)
		{
			Add(bunch.Item, bunch.Amount);
		}

		/// <summary>
		/// Adds the amount of items from another dictionary.
		/// </summary>
		public void Add(IDictionary<T, int> bag)
		{
			foreach (var field in bag)
				Add(field);
		}

		/// <summary>
		/// Removes the amount of items.
		/// </summary>
		public bool Remove(T item, int amount = 1)
		{
			if (!Contains(item, amount))
				return false;

			this[item] -= amount;

			return true;
		}

		/// <summary>
		/// Removes the amount of items.
		/// </summary>
		public bool Remove(KeyValuePair<T, int> kvp)
		{
			return Remove(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Removes the amount of items.
		/// </summary>
		public bool Remove(Bunch<T> bunch)
		{
			return Remove(bunch.Item, bunch.Amount);
		}

		/// <summary>
		/// Removes the amount of items from another dictionary.
		/// </summary>
		public bool Remove(IDictionary<T, int> bag)
		{
			if (!Contains(bag))
				return false;

			foreach ((T key, int value) in bag)
				this[key] -= value;

			return true;
		}

		/// <summary>
		/// Sets the amount of specified items to 0.
		/// </summary>
		public bool RemoveAll(T item)
		{
			if (!ContainsKey(item))
				return false;

			this[item] = 0;

			return true;
		}

		/// <summary>
		/// Sets the amount of specified items to 0.
		/// </summary>
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


		/// <summary>
		/// Returns whether it contains the specified amount of an item.
		/// </summary>
		public bool Contains(T item, int amount)
		{
			return this[item] >= amount;
		}

		/// <summary>
		/// Returns whether it contains the specified amount of an item.
		/// </summary>
		public bool Contains(Bunch<T> bunch)
		{
			return Contains(bunch.Item, bunch.Amount);
		}

		/// <summary>
		/// Returns whether it contains the specified amount of an item.
		/// </summary>
		public bool Contains(KeyValuePair<T, int> kvp)
		{
			return Contains(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Returns whether it contains the specified amounts of items.
		/// </summary>
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

		/// <summary>
		/// Convert the dictionary to a enumerable of Bunches.
		/// </summary>
		public IEnumerable<Bunch<T>> AsBunches()
		{
			return this.Select(kvp => new Bunch<T>(kvp));
		}

		/// <summary>
		/// Convert the dictionary to a List of Bunches.
		/// </summary>
		public List<Bunch<T>> ToBunches()
		{
			return AsBunches().ToList();
		}
	}

	/// <summary>
	/// <see cref="Bunch{T}"/> is just <c>KeyValuePair&lt;<typeparamref name="T"/>, int&gt;</c> with operators for use with Bags
	/// (would've just derived from <c>KeyValuePair</c> but you can't inherit structs).
	/// </summary>
	/// <example>
	/// <code>
	/// Bunch&lt;Currency&gt; base = new Bunch&lt;Currency&gt;(Currency.Diamonds, 10);
	/// Bunch&lt;Currency&gt; bonus = base * 4;
	/// </code>
	/// </example>
	[Serializable]
	public struct Bunch<T>
	{
		/// <summary>
		/// The item in question.
		/// </summary>
		public T Item;

		/// <summary>
		/// The number of items.
		/// </summary>
		public int Amount;

		public Bunch(KeyValuePair<T, int> pair): this(pair.Key, pair.Value)
		{
		}

		public Bunch(Bunch<T> field): this(field.Item, field.Amount)
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

		/// <summary>
		/// Convert to <c>KeyValuePair&lt;<typeparamref name="T" />, int&gt;</c>.
		/// </summary>
		public KeyValuePair<T, int> ToKVP()
		{
			return new KeyValuePair<T, int>(Item, Amount);
		}
	}
}