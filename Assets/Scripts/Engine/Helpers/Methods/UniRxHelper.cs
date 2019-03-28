using UniRx.Async;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

namespace Engine
{
	public static class UniRxHelper
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void FixUniRxWithECS()
		{
			PlayerLoopSystem playerLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
			PlayerLoopHelper.Initialize(ref playerLoop);
		}
	}
}