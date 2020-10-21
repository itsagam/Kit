using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	// Allows to turn a panel on/off with the Toggle
	[RequireComponent(typeof(Toggle))]
	public class TogglePanel: ToggleBehaviour
	{
		public RectTransform Panel;

		protected override void SetValue(bool value)
		{
			Panel.gameObject.SetActive(value);
		}
	}
}