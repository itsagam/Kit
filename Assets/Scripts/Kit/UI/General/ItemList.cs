using System.Collections;
using System.Linq;
using UnityEngine;

namespace Kit.UI
{
	/// <summary>Creates UI elements for a list of items. Instantiates an instance of <see cref="Item" /> for each one specified.</summary>
	public class ItemList: MonoBehaviour
	{
		/// <summary>The prefab to use for instantiating UI elements.</summary>
		[Tooltip("The prefab to use for instantiating UI elements.")]
		public Item Prefab;

		protected IEnumerable data;

		/// <summary>Destroys children and creates UI elements based on the <see cref="Data" /> property.</summary>
		public virtual void Refresh()
		{
			Clear();
			if (Data == null)
				return;
			foreach (object dataItem in Data)
			{
				Item item = Instantiate(Prefab, transform, false);
				item.Data = dataItem;
			}
		}

		/// <summary>Destroys all children.</summary>
		public virtual void Clear()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
				DestroyImmediate(transform.GetChild(i).gameObject);
		}

		/// <summary>Returns the first UI element.</summary>
		public virtual Item GetFirstItem()
		{
			return transform.GetFirstChild().GetComponent<Item>();
		}

		/// <summary>Returns the first UI element.</summary>
		public virtual T GetFirstItem<T>() where T: Item
		{
			return transform.GetFirstChild().GetComponent<T>();
		}

		/// <summary>Returns the last UI element.</summary>
		public virtual Item GetLastItem()
		{
			return transform.GetLastChild().GetComponent<Item>();
		}

		/// <summary>Returns the last UI element.</summary>
		public virtual T GetLastItem<T>() where T: Item
		{
			return transform.GetLastChild().GetComponent<T>();
		}

		/// <summary>Returns the UI element at a particular index.</summary>
		public virtual Item GetItem(int index)
		{
			return IsValid(index) ? transform.GetChild(index).GetComponent<Item>() : null;
		}

		/// <summary>Returns the UI element at a particular index, or <see langword="null" /> if <paramref name="index" /> is out-of-bounds.</summary>
		public virtual T GetItem<T>(int index) where T: Item
		{
			return IsValid(index) ? transform.GetChild(index).GetComponent<T>() : null;
		}

		/// <summary>Returns the list of UI elements.</summary>
		public virtual Item[] GetItems()
		{
			return GetComponentsInChildren<Item>(true);
		}

		/// <summary>Returns the list of UI elements.</summary>
		public virtual T[] GetItems<T>() where T: Item
		{
			return GetComponentsInChildren<T>(true);
		}

		/// <summary>Returns whether a particular index is out-of-range for elements created.</summary>
		public virtual bool IsValid(int index)
		{
			return index >= 0 && index < Count;
		}

		/// <summary>Returns the index of a particular UI element, or -1 if not found.</summary>
		public virtual int IndexOf(Item item)
		{
			Item found = GetComponentsInChildren<Item>(true).FirstOrDefault(i => i == item);
			if (found != null)
				return found.transform.GetSiblingIndex();
			return -1;
		}

		/// <summary>Returns the UI element at a particular index, or <see langword="null" /> if <paramref name="index" /> is out-of-bounds.</summary>
		public virtual Item this[int index] => GetItem(index);

		/// <summary>Returns the total number of UI elements.</summary>
		public virtual int Count => transform.childCount;

		/// <summary>Set a list of items and create UI elements for them, or get the list of items for which the elements were created.</summary>
		public virtual IEnumerable Data
		{
			get => data;
			set
			{
				data = value;
				Refresh();
			}
		}
	}
}