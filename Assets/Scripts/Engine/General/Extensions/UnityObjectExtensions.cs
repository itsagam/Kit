using UnityEngine;

public static class UnityObjectExtensions
{
	public static void Destroy(this Object obj)
	{
		Object.Destroy(obj);
	}
}