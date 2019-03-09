using UnityEngine;

public static class MonoExtensions
{
    public static void Enable(this MonoBehaviour mono)
    {
        mono.enabled = true;
    }

    public static void Disable(this MonoBehaviour mono)
    {
        mono.enabled = false;
    }
}