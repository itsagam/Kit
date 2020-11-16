using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Canvas" /> extensions.</summary>
	public static class CanvasExtensions
	{
		/// <summary>Returns whether the <see cref="Canvas" /> is rendering in screen-space.</summary>
		public static bool IsScreenSpace(this Canvas canvas)
		{
			RenderMode renderMode = canvas.renderMode;
			return renderMode == RenderMode.ScreenSpaceOverlay ||
				   renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null;
		}
	}
}