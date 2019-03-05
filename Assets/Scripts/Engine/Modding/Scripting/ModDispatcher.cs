using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
	public class ModDispatcher : MonoBehaviour
	{
		private static Transform Parent;

		void CreateParent()
		{
			GameObject parentGO = new GameObject("Mods");
			GameObject.DontDestroyOnLoad(parentGO);
			Parent = parentGO.transform;
		}

		void Awake()
		{
			if (Parent == null)
				CreateParent();

			transform.parent = Parent;
		}

		public void Destroy()
		{
			StopAllCoroutines();
			Destroy(gameObject);
		}
	}
}