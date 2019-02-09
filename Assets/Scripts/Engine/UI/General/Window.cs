#define CODE_ANALYSIS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

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
		UIManager.RegisterWindow(this);
    }

	public virtual async UniTask<bool> Show(object data = null, string animation = UIManager.DefaultShowAnimation)
	{
		if (IsBusy)
			return false;

		if (IsShown)
			return true;

		State = WindowState.Showing;
		OnShowing();
		OnWindowShowing?.Invoke();
		UIManager.Windows.Add(this);
		
		Data = data;
		gameObject.SetActive(true);

		if (animator != null && animation != null)
		{
			int animationHash = Animator.StringToHash(animation);
			if (animator.HasState(0, animationHash))
			{
				animator.Play(animationHash);
				//animator.Update(0);
				await Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
			}
		}
		UIManager.Play(transform, ShowSound);

		onShown();

		return true;
	}

	public virtual async UniTask<bool> Hide(WindowHideMode mode = UIManager.DefaultWindowHideMode, string animation = UIManager.DefaultHideAnimation)
    {
		if (IsBusy)
			return false;

        if (IsHidden)
            return true;

        State = WindowState.Hiding;
        OnHiding();
		OnWindowHiding?.Invoke();
		
		if (animator != null && animation != null)
        {
            int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                //animator.Update(0);
				await Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
            }
        }
		UIManager.Play(transform, HideSound);

		onHidden(mode);

		return true;
    }

	private void onShown()
	{
		State = WindowState.Shown;
		OnShown();
		OnWindowShown?.Invoke();
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
		OnWindowHidden?.Invoke();
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

    public virtual void Reload()
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
			Reload();
		}
	}
	#endregion
}