using UnityEngine;

namespace Engine
{
	public static class CanvasExtensions
	{
		/// <summary>
		/// Returns whether the Canvas is rendering in Screen-Space.
		/// </summary>
		public static bool IsScreenSpace(this Canvas canvas)
		{
			RenderMode renderMode = canvas.renderMode;
			return renderMode == RenderMode.ScreenSpaceOverlay ||
				   renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null;
		}
	}
}