using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IconList : MonoBehaviour
{
    public Icon Prefab;
    protected IEnumerable items;

    public virtual void Reload()
    {
        Clear();
        if (Items != null)
        {
            foreach (object item in Items)
            {
                Icon icon = Instantiate(Prefab);
                icon.transform.SetParent(transform, false);
                icon.Data = item;
            }
        }
    }

    public virtual void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    public virtual Icon FirstIcon()
    {
        return GetComponentInChildren<Icon>();
    }

    public virtual Icon[] Icons()
    {
        return GetComponentsInChildren<Icon>();
    }

    public virtual T FirstIcon<T>()
    {
        return GetComponentInChildren<T>();
    }

    public virtual T[] Icons<T>()
    {
        return GetComponentsInChildren<T>();
    }

    public virtual bool IsValid(int i)
    {
        return i >= 0 && i < Count;
    }

    public virtual int IndexOf(Icon icon)
    {
        Icon found = transform.GetComponentsInChildren<Icon>(true).FirstOrDefault(i => i == icon);
        if (found != null)
            return found.transform.GetSiblingIndex();
        return -1;
    }

    public virtual Icon this [int index]
    {
        get
        {
            if (IsValid(index))
            {
                Transform child = transform.GetChild(index);
                if (child != null)
                    return child.GetComponent<Icon>();
            }
            return null;
        }
    }

    public virtual int Count
    {
        get
        {
            return transform.childCount;
        }
    }

    public virtual IEnumerable Items
    {
        get
        {
            return items;
        }
        set
        {
            items = value;
            Reload();
        }
    }
}