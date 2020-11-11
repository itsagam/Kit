using UnityEngine;

namespace Engine.UI
{
	/// <summary>
	/// A parent class for UI elements that wish to display data.
	/// </summary>
	public class Item: MonoBehaviour
	{
		protected object data;

		/// <summary>
		/// Method that gets called when <see cref="Data"/> is updated. Child classes should override this method and update the UI here.
		/// </summary>
		public virtual void Refresh()
		{
		}

		/// <summary>
		/// Gets or sets element data. Calls <see cref="Refresh"/> when setting so the UI updates.
		/// </summary>
		public virtual object Data
		{
			get => data;
			set
			{
				data = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the transform index. Useful to re-order if the element is a part of a layout.
		/// </summary>
		public virtual int Index
		{
			get => transform.GetSiblingIndex();
			set => transform.SetSiblingIndex(value);
		}
	}
}