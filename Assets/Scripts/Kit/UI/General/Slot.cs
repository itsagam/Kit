using UnityEngine;
using UnityEngine.EventSystems;

namespace Kit.UI
{
	/// <summary>A UI element that can hold an <see cref="Item" /> inside it.</summary>
	public class Slot: MonoBehaviour, IDropHandler
	{
		/// <summary>The prefab to use for showing data in this slot.</summary>
		[Tooltip("The prefab to use for showing data in this slot.")]
		public Item Prefab;

		/// <summary>The <see cref="Item" /> fitted in this slot.</summary>
		public Item Item { get; protected set; }

		public virtual void OnDrop(PointerEventData eventData)
		{
			if (HasItem)
				return;

			DragCursor cursor = eventData.pointerDrag.GetComponent<DragCursor>();
			if (cursor == null || cursor.Item == null)
				return;

			if (!CanReceive(cursor.Item))
				return;

			Receive(cursor.Item);
		}

		/// <summary>Decides whether this slot can receive a particular item. To be overriden in child classes.</summary>
		public virtual bool CanReceive(Item item)
		{
			return true;
		}

		/// <summary>Fit an item into this slot.</summary>
		public virtual void Receive(Item newItem)
		{
			if (newItem == null)
			{
				Clear();
				return;
			}

			if (!HasItem)
				Item = Instantiate(Prefab, transform, false);

			Item.Data = newItem.Data;
		}

		/// <summary>Clear this slot.</summary>
		public virtual void Clear()
		{
			if (!HasItem)
				return;

			Item.gameObject.Destroy();
			Item = null;
		}

		/// <summary>Returns whether this slot is filled.</summary>
		public virtual bool HasItem => Item != null;
	}
}