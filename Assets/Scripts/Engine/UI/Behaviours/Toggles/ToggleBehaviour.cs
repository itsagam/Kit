using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	// Allows to set on/off color of a Toggle
	[RequireComponent(typeof(Toggle))]
	public abstract class ToggleBehaviour: MonoBehaviour
	{
		protected abstract void SetValue(bool value);
		protected Toggle toggle;

		protected virtual void Awake()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(SetValue);
			SetValue(toggle.isOn);
		}
	}
}