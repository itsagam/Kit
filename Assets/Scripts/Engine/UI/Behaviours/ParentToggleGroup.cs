using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	// Sets ToggleGroup of a Toggle from parent
	[RequireComponent(typeof(Toggle))]
	public class ToggleGroupFromParent : MonoBehaviour
	{
		protected void Awake()
		{
			GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
		}
	}
}