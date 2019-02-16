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

	protected void Awake()
	{
	}

	/*
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
	}
	*/

	public void Button()
	{
		Debugger.StartProfile(".transform");
		for (int i = 0; i < 10000; i++)
			Cube.transform.position = Vector3.one;
		Debugger.EndProfile();

		Transform t = Cube.transform;

		Debugger.StartProfile("Cached");
		for (int i = 0; i < 10000; i++)
			t.position = Vector3.one;
		Debugger.EndProfile();
	}
}
