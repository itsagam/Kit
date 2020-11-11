using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Parent class for behaviours that want to react to a <see cref="UnityEngine.UI.Toggle" />'s value.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public abstract class ToggleBehaviour: MonoBehaviour
	{
		protected abstract void OnValueChanged(bool value);
		protected Toggle toggle;

		protected virtual void Awake()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(OnValueChanged);
			OnValueChanged(toggle.isOn);
		}
	}
}