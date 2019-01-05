using System;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
	protected void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}