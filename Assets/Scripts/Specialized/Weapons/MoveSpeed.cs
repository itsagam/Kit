using System;
using Unity.Entities;

namespace Weapons
{
	[Serializable]
	public struct MoveSpeed : IComponentData
	{
		public float Speed;
	}
}