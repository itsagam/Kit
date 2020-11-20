using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Kit.UI
{
	/// <summary>
	///     An in-game wizard where each screen is a <see cref="Window" /> (this is so each screen can have their own animations,
	///     events and data if desired).
	/// </summary>
	public class Wizard: Window
	{
		/// <summary>List of all <see cref="Window" />s to use as wizard screens. Order is taken into account.</summary>
		[PropertyOrder(-2)]
		[Tooltip("List of all Windows to use as wizard screens. Order is taken into account.")]
		[SceneObjectsOnly]
		[Required]
		public List<Window> Screens;

		/// <summary>Index of the initial screen.</summary>
		[PropertyOrder(-1)]
		[Tooltip("Index of the initial screen.")]
		[PropertyRange(0, "@Count - 1")]
		public int DefaultScreen = 0;

		/// <summary>The animation state to play for showing the next screen.</summary>
		[Tooltip("The animation state to play for showing the next screen.")]
		[FoldoutGroup("Animations")]
		public string NextShowAnimation = "NextShow";

		/// <summary>The animation state to play for hiding the next screen.</summary>
		[Tooltip("The animation state to play for hiding the next screen.")]
		[FoldoutGroup("Animations")]
		public string NextHideAnimation = "NextHide";

		/// <summary>The animation state to play for showing the previous screen.</summary>
		[Tooltip("The animation state to play for showing the previous screen.")]
		[FoldoutGroup("Animations")]
		public string PreviousShowAnimation = "PreviousShow";

		/// <summary>The animation state to play for hiding the previous screen.</summary>
		[Tooltip("The animation state to play for hiding the previous screen.")]
		[FoldoutGroup("Animations")]
		public string PreviousHideAnimation = "PreviousHide";

		/// <summary>Stuff to do when changing screens.</summary>
		[Tooltip("Stuff to do when changing screens.")]
		[FoldoutGroup("Events")]
		public ChangeEvent Changing;

		/// <summary>Stuff to do when the active screen has changed.</summary>
		[Tooltip("Stuff to do when the current screen has changed.")]
		[FoldoutGroup("Events")]
		public ChangeEvent Changed;

		/// <summary>
		///     Class for screen change events. Parameters are: previous screen index, previous screen, new screen index and new screen,
		///     in that order.
		/// </summary>
		[Serializable]
		public class ChangeEvent: UnityEvent<int, Window, int, Window>
		{
		}

		/// <summary>Index of the active screen.</summary>
		public int Index { get; protected set; } = -1;

		protected override void Awake()
		{
			base.Awake();
			Screens.ForEach(screen => screen.gameObject.SetActive(false));
		}

		protected override void Start()
		{
			if (IsValid(DefaultScreen))
				GoTo(DefaultScreen).Forget();
		}

		/// <summary>Move the wizard to a specific step.</summary>
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

				var previousTask = previous == null ? Show() : previous.Hide(isNext ? NextHideAnimation : PreviousHideAnimation,
																			 WindowHideMode.Deactivate);
				var nextTask = next.Show(previous == null ? null :
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

		/// <summary>Move the wizard to a specific screen.</summary>
		public virtual UniTask<bool> GoTo(Window window)
		{
			int i = IndexOf(window);
			return i >= 0 ? GoTo(i) : UniTask.FromResult(false);
		}

		/// <summary>Move the wizard forward.</summary>
		public virtual UniTask<bool> Next()
		{
			return GoTo(Index + 1);
		}

		/// <summary>Move the wizard backward.</summary>
		public virtual UniTask<bool> Previous()
		{
			return GoTo(Index - 1);
		}

		/// <summary>Returns whether a particular index is valid.</summary>
		public virtual bool IsValid(int index)
		{
			return index >= 0 && index < Count;
		}

		/// <summary>Returns the index of a particular screen.</summary>
		public virtual int IndexOf(Window window)
		{
			return Screens.IndexOf(window);
		}

		/// <summary>Returns the active screen.</summary>
		public virtual Window Active => this[Index];

		/// <summary>Returns the screen at particular index.</summary>
		public virtual Window this[int index] => IsValid(index) ? Screens[index] : null;

		/// <summary>Returns the total number of screens.</summary>
		public virtual int Count => Screens.Count;
	}
}