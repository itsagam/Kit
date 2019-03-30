using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Rendering
{
	[Serializable]
	public struct SpriteInstanceRenderer : ISharedComponentData
	{
		public Texture2D Sprite;
		public float PixelsPerUnit;
		public float2 Pivot;

		public SpriteInstanceRenderer(Texture2D sprite, int pixelsPerUnit, float2 pivot)
		{
			Sprite = sprite;
			PixelsPerUnit = pixelsPerUnit;
			Pivot = pivot;
		}
	}
}