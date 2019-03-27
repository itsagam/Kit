using Unity.Entities;
using UnityEngine;

namespace Weapons
{
	[RequiresEntityConversion]
	public class MoveSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
	{
		public float Speed = 20;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			MoveSpeed data = new MoveSpeed { Speed = Speed };
			dstManager.AddComponentData(entity, data);
		}
	}
}