using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Allows to turn a panel on/off with a <see cref="UnityEngine.UI.Toggle" />.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class TogglePanel: ToggleBehaviour
	{
		public RectTransform Panel;

		protected override void OnValueChanged(bool value)
		{
			Panel.gameObject.SetActive(value);
		}
	}
}