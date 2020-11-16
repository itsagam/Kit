using UnityEngine;

namespace Kit.UI
{
	/// <summary>An <see cref="ItemList" /> that creates UI elements based on objects directly specified in the inspector.</summary>
	public class ObjectList: ItemList
	{
		/// <summary>Objects to create UI elements for.</summary>
		[Tooltip("Objects to create UI elements for.")]
		public Object[] Objects;

		public virtual void Start()
		{
			if (Objects != null)
				Data = Objects;
		}
	}
}