using System;
using Sirenix.OdinInspector;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;

namespace Engine.UI
{
	public class Window : MonoBehaviour
	{
		[FoldoutGroup("Animations")]
		public string ShowAnimation = "Show";
		[FoldoutGroup("Animations")]
		public string HideAnimation = "Hide";

		[FoldoutGroup("Sounds")]
		public AudioClip ShowSound;
		[FoldoutGroup("Sounds")]
		public AudioClip HideSound;

		[FoldoutGroup("Events")]
		public UnityEvent Showing;
		[FoldoutGroup("Events")]
		public UnityEvent Shown;
		[FoldoutGroup("Events")]
		public UnityEvent Hiding;
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

		public UniTask<bool> Show(object data = null)
		{
			return Show(ShowAnimation, data);
		}

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

			AudioManager.PlayUIEffect(ShowSound);
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

		public UniTask<bool> Hide(WindowHideMode mode = UIManager.DefaultHideMode)
		{
			return Hide(HideAnimation, mode);
		}

		public async UniTask<bool> Hide(string animation, WindowHideMode mode = UIManager.DefaultHideMode)
		{
			if (IsBusy)
				return false;

			if (IsHidden)
				return true;

			State = WindowState.Hiding;
			OnHiding();
			Hiding.Invoke();

			AudioManager.PlayUIEffect(HideSound);
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
			if (mode == WindowHideMode.Destroy || (mode == WindowHideMode.Auto && isInstance))
				Destroy(gameObject);
			else
			{
				gameObject.SetActive(false);
				UIManager.Windows.Remove(this);
			}

			OnHidden();
			Hidden.Invoke();
		}

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
		protected virtual void OnShowing()
		{
		}

		protected virtual void OnShown()
		{

		}

		protected virtual void OnHiding()
		{
		}

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