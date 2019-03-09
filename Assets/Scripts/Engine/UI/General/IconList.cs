using System.Collections;
using System.Linq;
using UnityEngine;

public class IconList: MonoBehaviour
{
	public Icon Prefab;
	protected IEnumerable items;

	public virtual void Refresh()
	{
		Clear();
		if (Items == null)
			return;
		foreach (object item in Items)
		{
			Icon icon = Instantiate(Prefab, transform, false);
			icon.Data = item;
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

	public virtual Icon this[int index] => IsValid(index) ? transform.GetChild(index).GetComponent<Icon>() : null;
	public virtual int Count => transform.childCount;

	public virtual IEnumerable Items
	{
		get => items;
		set
		{
			items = value;
			Refresh();
		}
	}
}