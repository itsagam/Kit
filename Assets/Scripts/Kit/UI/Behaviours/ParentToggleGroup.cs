using UnityEngine;
using UnityEngine.UI;

namespace Kit.UI.Behaviours
{
	/// <summary>Sets the <see cref="ToggleGroup" /> of a <see cref="Toggle" /> from its parent.</summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleGroupFromParent: MonoBehaviour
	{
		protected void Awake()
		{
			GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
		}
	}
}