using System;
using UniRx;
using UniRx.Async;
using UnityEngine;

public class Test : MonoBehaviour
{
	public void Button()
	{

	}

	protected async UniTask DoSomething()
	{
		await Observable.Timer(TimeSpan.FromSeconds(1));
		print("Done something");
	}
}
