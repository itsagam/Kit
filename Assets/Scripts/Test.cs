using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	public Parent Parent;
	public Child Child;

	protected void Awake()
	{
		/*
		var group = Pooler.CreateGroup("Effects");
		group.Organize = false;
		group.Persistent = true;

		var parent1 = Pooler.Instantiate("Effects", Parent, Vector3.one, Quaternion.identity);
		var parent2 = Pooler.Instantiate("Effects", Parent, Vector3.one, Quaternion.identity);
		var parent3 = Pooler.Instantiate("Effects", Parent, Vector3.one, Quaternion.identity);
		var child1 = Pooler.Instantiate("Effects", Child, Vector3.one, Quaternion.identity);
		var child2 = Pooler.Instantiate("Effects", Child, Vector3.one, Quaternion.identity);
		*/
	}

	public void Button()
	{
		//Debugger.StartProfile("Class");
		//for (int i = 0; i < 1000000; i++)
		//Debugger.EndProfile();

		
		//Debugger.StartProfile("Int");
		//for (int i = 0; i < 1000000; i++)
		//Debugger.EndProfile();
	}
}
