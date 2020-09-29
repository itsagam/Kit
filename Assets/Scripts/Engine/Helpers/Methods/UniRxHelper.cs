/*
using Cysharp.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Engine
{
	public static class UniRxHelper
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void FixUniRxWithECS()
		{
			UnityEngine.LowLevel.PlayerLoopSystem playerLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
			PlayerLoopHelper.Initialize(ref playerLoop);
		}
	}
}
*/