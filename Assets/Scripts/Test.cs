using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Modding;
using XLua;
using Cards;

public class Test : MonoBehaviour
{
	protected void Awake()
	{
		new CardSet().Fill().Shuffle().SortByRank().Log();
	}

	protected void Start()
	{
	}

	public void Button()
	{
	}
}
