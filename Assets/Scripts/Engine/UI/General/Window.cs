using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Engine.UI
{
	/// <summary>
	/// Base class for all screens. Handles animation, sound and events.
	/// </summary>
	public class Window: MonoBehaviour
	{
		/// <summary>
		/// The animation state to play when showing the screen.
		/// </summary>
		[Toggle("The animation state to play when showing the screen.")]
		[FoldoutGroup("Animations")]
		public string ShowAnimation = "Show";

		/// <summary>
		/// The animation state to play when hiding the screen.
		/// </summary>
		[Toggle("The animation state to play when hiding the screen.")]
		[FoldoutGroup("Animations")]
		public string HideAnimation = "Hide";

		/// <summary>
		/// The audio to play when showing.
		/// </summary>
		[FoldoutGroup("Sounds")]
		[Tooltip("The audio to play when showing.")]
		public AudioClip ShowSound;

		/// <summary>
		/// The audio to play when hiding.
		/// </summary>
		[FoldoutGroup("Sounds")]
		[Tooltip("The audio to play when hiding.")]
		public AudioClip HideSound;

		/// <summary>
		/// Stuff to do when showing the screen.
		/// </summary>
		[Tooltip("Stuff to do when showing the screen.")]
		[FoldoutGroup("Events")]
		public UnityEvent Showing;

		/// <summary>
		/// Stuff to do when the screen has shown.
		/// </summary>
		[Tooltip("Stuff to do when the screen has shown.")]
		[FoldoutGroup("Events")]
		public UnityEvent Shown;

		/// <summary>
		/// Stuff to do when hiding the screen.
		/// </summary>
		[Tooltip("Stuff to do when hiding the screen.")]
		[FoldoutGroup("Events")]
		public UnityEvent Hiding;

		/// <summary>
		/// Stuff to do when the screen has hidden.
		/// </summary>
		[Tooltip("Stuff to do when the screen has hidden.")]
		[FoldoutGroup("Events")]
		public UnityEvent Hidden;

		public WindowState State { get; protected set; } = WindowState.Hidden;

		protected Animator animator;
		protected object data;
		protected bool isInstance = false;

		#region Functionality

		protected virtual void Awake()
		{
			animator = GetComponent<Animator>();
			gameObject.SetActive(false);
			UIManager.Register(this);
		}

		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="data">Data to pass to the window.</param>
		/// <returns>Whether the window was successfully shown.</returns>
		/// <remarks>Can be await-ed upon.</remarks>
		public UniTask<bool> Show(object data = null)
		{
			return Show(ShowAnimation, data);
		}

		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="animation">Play this animation state instead of the one set in <see cref="ShowAnimation"/>.</param>
		/// <param name="data">Data to pass to the window.</param>
		/// <returns>Whether the window was successfully shown.</returns>
		/// <remarks>Can be await-ed upon.</remarks>
		public async UniTask<bool> Show(string animation, object data = null)
		{
			if (IsBusy)
				return false;

			if (IsShown)
				return true;

			State = WindowState.Showing;
			OnShowing();
			Showing.Invoke();
			UIManager.Windows.Add(this);

			Data = data;
			gameObject.SetActive(true);

			AudioManager.PlayUI(ShowSound);
			if (animator != null && !animation.IsNullOrEmpty())
			{
				int animationHash = Animator.StringToHash(animation);
				if (animator.HasState(0, animationHash))
				{
					animator.Play(animationHash);
					//animator.Update(0);
					await UniTask.Delay(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
				}
			}

			OnShownInternal();

			return true;
		}

		/// <summary>
		/// Hide the window.
		/// </summary>
		/// <param name="mode">How to hide the window?</param>
		/// <returns>Whether the window was successfully hidden.</returns>
		/// <remarks>Can be await-ed upon.</remarks>
		public UniTask<bool> Hide(WindowHideMode mode = UIManager.DefaultHideMode)
		{
			return Hide(HideAnimation, mode);
		}

		/// <summary>
		/// Hide the window.
		/// </summary>
		/// <param name="animation">Play this animation state instead of the one set in <see cref="HideAnimation"/>.</param>
		/// <param name="mode">How to hide the window?</param>
		/// <returns>Whether the window was successfully hidden.</returns>
		/// <remarks>Can be await-ed upon.</remarks>
		public async UniTask<bool> Hide(string animation, WindowHideMode mode = UIManager.DefaultHideMode)
		{
			if (IsBusy)
				return false;

			if (IsHidden)
				return true;

			State = WindowState.Hiding;
			OnHiding();
			Hiding.Invoke();

			AudioManager.PlayUI(HideSound);
			if (animator != null && !animation.IsNullOrEmpty())
			{
				int animationHash = Animator.StringToHash(animation);
				if (animator.HasState(0, animationHash))
				{
					animator.Play(animationHash);
					//animator.Update(0);
					await UniTask.Delay(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
				}
			}

			OnHiddenInternal(mode);

			return true;
		}

		private void OnShownInternal()
		{
			State = WindowState.Shown;
			OnShown();
			Shown.Invoke();
		}

		private void OnHiddenInternal(WindowHideMode mode)
		{
			State = WindowState.Hidden;
			data = null;
			if (mode == WindowHideMode.Destroy || mode == WindowHideMode.Auto && isInstance)
				gameObject.Destroy();
			else
			{
				gameObject.SetActive(false);
				UIManager.Windows.Remove(this);
			}

			OnHidden();
			Hidden.Invoke();
		}

		/// <summary>
		/// Mark the window as one not already in the scene. Called automatically.
		/// </summary>
		/// <remarks>
		/// Used to decide the behaviour when <see cref="WindowHideMode"/> is <see cref="WindowHideMode.Auto"/>.
		/// </remarks>
		public void MarkAsInstance()
		{
			isInstance = true;
		}

		protected virtual void OnDestroy()
		{
			UIManager.Windows.Remove(this);
		}

		#endregion

		#region Extendable functions

		/// <summary>
		/// Child classes that want to do stuff while the window is showing should extend this method.
		/// </summary>
		protected virtual void OnShowing()
		{
		}

		/// <summary>
		/// Child classes that want to do stuff when the window has shown should extend this method.
		/// </summary>
		protected virtual void OnShown()
		{
		}

		/// <summary>
		/// Child classes that want to do stuff while the window is hiding should extend this method.
		/// </summary>
		protected virtual void OnHiding()
		{
		}

		/// <summary>
		/// Child classes that want to do stuff when the window has hidden should extend this method.
		/// </summary>
		protected virtual void OnHidden()
		{
		}

		public virtual void Refresh()
		{
		}

		#endregion

		#region Public functions

		public virtual bool IsBusy => State == WindowState.Showing || State == WindowState.Hiding;

		public virtual bool IsShown => State == WindowState.Shown;

		public virtual bool IsHidden => State == WindowState.Hidden;

		public virtual object Data
		{
			get => data;
			set
			{
				data = value;
				Refresh();
			}
		}

		#endregion
	}
}