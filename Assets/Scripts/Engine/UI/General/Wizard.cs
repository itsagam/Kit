using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Engine.UI
{
	public class Wizard: Window
	{
		[PropertyOrder(-99)]
		public Window Default;

		[FoldoutGroup("Animations")]
		public string NextShowAnimation = "NextShow";

		[FoldoutGroup("Animations")]
		public string NextHideAnimation = "NextHide";

		[FoldoutGroup("Animations")]
		public string PreviousShowAnimation = "PreviousShow";

		[FoldoutGroup("Animations")]
		public string PreviousHideAnimation = "PreviousHide";

		[FoldoutGroup("Events")]
		public ChangeEvent Changing;

		[FoldoutGroup("Events")]
		public ChangeEvent Changed;

		[Serializable]
		public class ChangeEvent: UnityEvent<int, Window, int, Window>
		{
		}

		public int Index { get; protected set; } = -1;

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

				var previousTask = previous == null ? Show() : previous.Hide(isNext ? NextHideAnimation : PreviousHideAnimation);
				var nextTask = next.Show(null,
										 previous == null ? null :
										 isNext           ? NextShowAnimation : PreviousShowAnimation);

				Changing?.Invoke(previousIndex, previous, Index, next);
				await UniTask.WhenAll(previousTask, nextTask);
				Changed?.Invoke(previousIndex, previous, Index, next);
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
			return i >= 0 ? GoTo(i) : UniTask.FromResult(false);
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

		public virtual Window Active => this[Index];
		public virtual Window this[int index] => IsValid(index) ? transform.GetChild(index).GetComponent<Window>() : null;
		public virtual int Count => transform.childCount;
	}
}