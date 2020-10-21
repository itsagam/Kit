using UnityEngine;

namespace Engine.UI.Behaviours
{
	public class IgnoreRaycast: MonoBehaviour, ICanvasRaycastFilter
	{
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return false;
		}
	}
}