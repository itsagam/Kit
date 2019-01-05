using System;
using UnityEngine;

public static class CanvasExtensions
{
	public static bool IsScreenSpace(this Canvas canvas)
	{
		return
			canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
			(canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null);
	}
}