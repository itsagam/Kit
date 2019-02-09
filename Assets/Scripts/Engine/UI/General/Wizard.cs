using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;

public class Wizard : Window
{
	public Window Default;
	public string NextShowAnimation = "NextShow";
	public string NextHideAnimation = "NextHide";
	public string PreviousShowAnimation = "PreviousShow";
	public string PreviousHideAnimation = "PreviousHide";

	public int Index { get; protected set; } = -1;
	public event Action<int, Window, int, Window> OnChange;

	protected void Start()
	{
		if (Default != null)
			GoTo(Default);
	}

	public virtual async UniTask<bool> GoTo(int index)
	{		
		if (IsBusy)
			return false;

		if (index == Index)
			return true;

		Window previous = Active;
		if (previous != null && previous.IsBusy)
			return false;

		if (IsValid(index))
		{
			Window next = this[index];
			if (next == null || next.IsBusy)
				return false;

			bool isNext = index > Index;
			int previousIndex = Index;
			Index = index;

			UniTask<bool> previousTask;
			if (previous == null)
				previousTask = Show();
			else
				previousTask = previous.Hide(WindowHideMode.Auto, isNext ? NextHideAnimation : PreviousHideAnimation);

			UniTask<bool> nextTask = next.Show(null, previous == null ? null : (isNext ? NextShowAnimation : PreviousShowAnimation));

			await UniTask.WhenAll(previousTask, nextTask);

			OnChange?.Invoke(previousIndex, previous, Index, next);
		}
		else
		{
			Index = index;
			await Hide();
		}
		return true;
	}

	public virtual UniTask<bool> GoTo(Window window)
	{
		int i = IndexOf(window);
		if (i >= 0)
			return GoTo(i);
		return UniTask.FromResult(false);
	}

	public virtual UniTask<bool> Next()
	{
		return GoTo(Index + 1);
	}

	public virtual UniTask<bool> Previous()
	{
		return GoTo(Index - 1);
	}

	public virtual bool IsValid(int index)
	{
		return index >= 0 && index < Count;
	}

	public virtual int IndexOf(Window window)
	{
		Window found = transform.GetComponentsInChildren<Window>(true).FirstOrDefault(p => p == window);
		if (found != null)
			return found.transform.GetSiblingIndex();
		return -1;
	}

	public virtual Window this[int index]
	{
		get
		{
			if (IsValid(index))
				return transform.GetChild(index)?.GetComponent<Window>();
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

	public virtual Window Active
	{
		get
		{
			return this[Index];
		}
	}
}