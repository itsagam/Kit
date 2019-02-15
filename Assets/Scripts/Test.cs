using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	public Transform Cube;

	async UniTask Start()
	{
		Component instance1 = Pooler.Spawn(Cube);
		Component instance2 = Pooler.Spawn(Cube);
		Component instance3 = Pooler.Spawn(Cube);
		await Observable.Timer(TimeSpan.FromSeconds(3));
		Pooler.Despawn(instance1);
		await Observable.Timer(TimeSpan.FromSeconds(3));
		Component instance4 = Pooler.Spawn(Cube);
		Debug.Log(instance1 == instance4);

		foreach(var instance in Pooler.GetPool(Cube))
		{
			print(instance);
		}
	}

	public void Button()
	{

	}
}
