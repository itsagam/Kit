using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parent : MonoBehaviour
{
	public int Value = 0;

	public void Call()
	{
		Value += 1;
	}
}