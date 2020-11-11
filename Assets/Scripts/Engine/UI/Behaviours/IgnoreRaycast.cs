using UnityEngine;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Sets a particular UI element to ignore raycasts and be un-interactable.
	/// </summary>
	public class IgnoreRaycast: MonoBehaviour, ICanvasRaycastFilter
	{
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return false;
		}
	}
}