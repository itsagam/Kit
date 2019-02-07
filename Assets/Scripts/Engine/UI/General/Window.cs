using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;

public class Window : MonoBehaviour
{
	public event Action OnWindowShowing;
	public event Action OnWindowShown;
	public event Action OnWindowHiding;
    public event Action OnWindowHidden;

	public AudioClip ShowSound;
	public AudioClip HideSound;
	public WindowState State { get; protected set; } = WindowState.Hidden;

	protected Animator animator;
	protected object data;
	protected bool isInstance = false;

	#region Functionality
	protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    public virtual bool Show(object data = null, Action onShown = null, string animation = UIManager.DefaultShowAnimation)
    {
        if (IsShown())
            return false;

		State = WindowState.Showing;
        Data = data;
		UIManager.Shown.Add(this);
		gameObject.SetActive(true);

		OnShowing();
		OnWindowShowing?.Invoke();
		//UIManager.OnWindowShowing?.Invoke(this);

		bool animated = false;
		if (animator != null && animation != null)
        {
			int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
				animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => this.onShown(onShown));
            }
        }
		UIManager.Play(transform, ShowSound);
		if (!animated)
			this.onShown(onShown);
        return true;
    }

	public virtual void Reshow(object data = null, Action onShown = null)
	{
		Data = data;
		UIManager.Play(transform, ShowSound);
		onShown?.Invoke();
	}

	public virtual bool Hide(Action onHidden = null, WindowHideMode mode = UIManager.DefaultWindowHideMode, string animation = UIManager.DefaultHideAnimation)
    {
        if (IsHidden())
            return false;

        State = WindowState.Hiding;
        OnHiding();
		OnWindowHiding?.Invoke();
		//UIManager.OnWindowHiding?.Invoke(this);
		bool animated = false;
		if (animator != null && animation != null)
        {
            int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
                animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => this.onHidden(onHidden, mode));
            }
        }
		UIManager.Play(transform, HideSound);
        if (!animated)
			this.onHidden(onHidden, mode);
        return true;
    }

	private void onShown(Action onShown)
	{
		State = WindowState.Shown;
		OnShown();
		onShown?.Invoke();
		OnWindowShown?.Invoke();
		//UIManager.OnWindowShown?.Invoke(this);
	}

	private void onHidden(Action onHidden, WindowHideMode mode)
	{
		data = null;
		State = WindowState.Hidden;
		if (mode == WindowHideMode.Destroy || (mode == WindowHideMode.Auto && isInstance))
		{
			Destroy(gameObject);
		}
		else
		{
			gameObject.SetActive(false);
			UIManager.Shown.Remove(this);
		}
		OnHidden();
		onHidden?.Invoke();
		OnWindowHidden?.Invoke();
		//UIManager.OnWindowHidden?.Invoke(this);
	}

	public virtual void MarkAsInstance()
	{
		isInstance = true;
	}

	protected virtual void OnDestroy()
	{
		UIManager.Shown.Remove(this);
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

    public virtual void Reload()
    {
    }
	#endregion

	#region Public functions
	public virtual bool IsBusy()
    {
		return State == WindowState.Showing || State == WindowState.Hiding;
    }

	public virtual bool IsShown()
	{
		return State == WindowState.Shown;
    }

    public virtual bool IsHidden()
    {
		return State == WindowState.Hidden;
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
			Reload();
		}
	}
	#endregion
}