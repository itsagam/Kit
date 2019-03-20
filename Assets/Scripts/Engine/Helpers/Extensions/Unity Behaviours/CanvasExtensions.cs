using UnityEngine;

namespace Engine
{
	public static class CanvasExtensions
	{
		public static bool IsScreenSpace(this Canvas canvas)
		{
			RenderMode renderMode = canvas.renderMode;
			return
				renderMode == RenderMode.ScreenSpaceOverlay ||
				renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null;
		}
	}
}