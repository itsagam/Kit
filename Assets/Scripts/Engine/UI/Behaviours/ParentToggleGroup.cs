using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Sets ToggleGroup of a Toggle from parent.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleGroupFromParent: MonoBehaviour
	{
		protected void Awake()
		{
			GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
		}
	}
}