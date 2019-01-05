using System;
using UnityEngine;

public class LoadOnce : MonoBehaviour
{
	public UnityEngine.Object Instance;

	protected void Awake()
	{
		UnityEngine.Object[] objects = FindObjectsOfType(Instance.GetType());
		if (objects.Length <= 1)
			DontDestroyOnLoad(gameObject);
		else
			Destroy(gameObject);
	}
}