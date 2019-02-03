using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonPrefabInstantiator
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PrefabAttribute : Attribute
	{
	}
}