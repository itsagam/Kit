using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Icon : MonoBehaviour
{
    protected object data;

    public virtual void Reload()
    {
    }

    public virtual object Data
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
            Reload();
        }
    }

    public virtual int Index
    {
        get
        {
            return transform.GetSiblingIndex();
        }
        set
        {
            transform.SetSiblingIndex(value);
        }
    }
}