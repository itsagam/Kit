using System.Collections.Generic;
using Engine;
using UnityEngine;

namespace Weapons.Modifiers.Impacters
{
	public class Pierce : IImpact
	{
		public LayerMask Layers = -5;
		public List<string> Tags = new List<string>();

		public bool OnImpact(Transform fireable, Transform impact, Vector3 position, Vector2 normal)
		{
			bool match = Layers.Contains(fireable.gameObject.layer);

			if (Tags.Count > 0)
				match = match && Tags.Contains(impact.tag);

			return !match;
		}
	}
}