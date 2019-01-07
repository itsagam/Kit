using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class Wizard : Popup
{
	public Popup Default;
	public string NextShowAnimation = "NextShow";
	public string NextHideAnimation = "NextHide";
	public string PreviousShowAnimation = "PreviousShow";
	public string PreviousHideAnimation = "PreviousHide";

	public int Index { get; protected set; } = -1;
	public event Action<int, Popup, int, Popup> OnChange;

	protected override void Awake()
	{
		base.Awake();
		Observable.NextFrame().Subscribe(t => {
				if (Default != null)
					GoTo(Default);
			});
	}

	public virtual bool GoTo(int index)
	{
		if (index == Index)
			return false;
		
		if (IsBusy())
			return false;

		Popup previous = Active;
		if (IsValid(index))
		{
			bool isNext = index > Index;
			int previousIndex = Index;
			if (previous != null)
			{
				if (!previous.IsBusy())
					previous.Hide(null, PopupHideMode.Auto, isNext ? NextHideAnimation : PreviousHideAnimation);
				else
					return false;
			}
			else
				Show();
			Index = index;
			Popup next = Active;
			next.Show(default, null, previous == null ? null : (isNext ? NextShowAnimation : PreviousShowAnimation));
			OnChange?.Invoke(previousIndex, previous, Index, next);
		}
		else
		{
			if (previous != null && !previous.IsBusy())
			{
				Hide();
				Index = index;
			}
			else
				return false;
		}
		return true;
	}

	public virtual bool GoTo(Popup popup)
	{
		int i = IndexOf(popup);
		if (i >= 0)
			return GoTo(i);
		return false;
	}

	public virtual bool Next()
	{
		return GoTo(Index + 1);
	}

	public virtual bool Previous()
	{
		return GoTo(Index - 1);
	}

	public virtual bool IsValid(int index)
	{
		return index >= 0 && index < Count;
	}

	public virtual bool IsValid()
	{
		return IsValid(Index);
	}

	public virtual int IndexOf(Popup popup)
	{
		Popup found = transform.GetComponentsInChildren<Popup>(true).FirstOrDefault(p => p == popup);
		if (found != null)
			return found.transform.GetSiblingIndex();
		return -1;
	}

	public virtual Popup this[int index]
	{
		get
		{
			if (IsValid(index))
			{
				Transform child = transform.GetChild(index);
				if (child != null)
					return child.GetComponent<Popup>();
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

	public virtual Popup Active
	{
		get
		{
			return this[Index];
		}
	}
}