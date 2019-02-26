using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Async;
using Sirenix.OdinInspector;

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
	public UnityEvent OnWindowShowing;
	[FoldoutGroup("Events")]
	public UnityEvent OnWindowShown;
	[FoldoutGroup("Events")]
	public UnityEvent OnWindowHiding;
	[FoldoutGroup("Events")]
	public UnityEvent OnWindowHidden;

	public WindowState State { get; protected set; } = WindowState.Hidden;

	protected Animator animator;
	protected object data;
	protected bool isInstance = false;

	#region Functionality
	protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
		UIManager.RegisterWindow(this);
    }

	public virtual UniTask<bool> Show(object data = null)
	{
		return Show(ShowAnimation, data);
	}

	public virtual async UniTask<bool> Show(string animation, object data = null)
	{
		if (IsBusy)
			return false;

		if (IsShown)
			return true;

		State = WindowState.Showing;
		OnShowing();
		OnWindowShowing.Invoke();
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
				await Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
			}
		}

		onShown();

		return true;
	}

	public virtual UniTask<bool> Hide(WindowHideMode mode = UIManager.DefaultWindowHideMode)
	{
		return Hide(HideAnimation, mode);
	}

	public virtual async UniTask<bool> Hide(string animation, WindowHideMode mode = UIManager.DefaultWindowHideMode)
    {
		if (IsBusy)
			return false;

        if (IsHidden)
            return true;

        State = WindowState.Hiding;
        OnHiding();
		OnWindowHiding.Invoke();

		AudioManager.PlayUIEffect(HideSound);
		if (animator != null && !animation.IsNullOrEmpty())
        {
            int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                //animator.Update(0);
				await Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
            }
        }

		onHidden(mode);

		return true;
    }

	private void onShown()
	{
		State = WindowState.Shown;
		OnShown();
		OnWindowShown.Invoke();
	}

	private void onHidden(WindowHideMode mode)
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
		OnWindowHidden.Invoke();
	}

	public virtual void MarkAsInstance()
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
	public virtual bool IsBusy
    {
		get
		{ 
			return State == WindowState.Showing || State == WindowState.Hiding;
		}
	}

	public virtual bool IsShown
	{
		get
		{
			return State == WindowState.Shown;
		}
    }

    public virtual bool IsHidden
    {
		get
		{ 
			return State == WindowState.Hidden;
		}
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
			Refresh();
		}
	}
	#endregion
}