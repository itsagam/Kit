using UnityEngine;

namespace Engine
{
    public static class BehaviourExtensions
    {
        public static void Enable(this Behaviour behaviour)
        {
            behaviour.enabled = true;
        }

        public static void Disable(this Behaviour behaviour)
        {
            behaviour.enabled = false;
        }
    }
}