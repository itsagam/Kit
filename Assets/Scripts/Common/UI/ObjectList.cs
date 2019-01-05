using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObjectList : IconList
{
    public UnityEngine.Object[] Objects;

    public virtual void Start()
    {
        if (Objects != null)
            Items = Objects;
    }
}