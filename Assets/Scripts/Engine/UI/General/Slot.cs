﻿using UnityEngine.EventSystems;

public class Slot: Icon, IDropHandler
{
	public Icon Prefab;
	public Icon Icon { get; protected set; }

	public virtual void OnDrop(PointerEventData eventData)
	{
		if (HasIcon)
			return;

		DragCursor cursor = eventData.pointerDrag.GetComponent<DragCursor>();
		if (cursor == null || cursor.Icon == null)
			return;

		if (!CanReceive(cursor.Icon))
			return;

		Receive(cursor.Icon);
	}

	public virtual bool CanReceive(Icon icon)
	{
		return true;
	}

	public virtual void Receive(Icon icon)
	{
		Data = icon.Data;
		Destroy(icon.gameObject);
	}

	public virtual void Clear()
	{
		Data = null;
	}

	public override void Refresh()
	{
		if (HasIcon)
		{
			Destroy(Icon.gameObject);
			Icon = null;
		}

		if (Data == null)
			return;

		Icon instance = Instantiate(Prefab, transform, false);
		instance.Data = Data;
		Icon = instance;
	}

	public virtual bool HasIcon => Icon != null;
}