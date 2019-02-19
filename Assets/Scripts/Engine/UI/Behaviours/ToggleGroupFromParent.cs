using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleGroupFromParent : MonoBehaviour
{
	protected void Awake()
	{
		GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
	}
}