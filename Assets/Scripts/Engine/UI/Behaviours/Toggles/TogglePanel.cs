using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Allows to turn a <see cref="Panel"/> on/off with a <see cref="UnityEngine.UI.Toggle" />.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class TogglePanel: ToggleBehaviour
	{
		/// <summary>
		/// The <see cref="Panel"/> to show or hide.
		/// </summary>
		[Tooltip("The Panel to show or hide.")]
		public RectTransform Panel;

		protected override void OnValueChanged(bool value)
		{
			Panel.gameObject.SetActive(value);
		}
	}
}