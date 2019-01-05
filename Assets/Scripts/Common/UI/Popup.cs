using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public enum PopupState
{
    Shown,
    Showing,
    Hidden,
    Hiding
}

public enum PopupShowMode
{
	New,
	Dont,
	Override,
	HidePrevious,
}

public enum PopupHideMode
{
	Auto,
	Deactivate,
	Destroy
}

public class Popup : MonoBehaviour
{
	//TODO: Simplify popup pathing
	public const string Path = "UI/";
	public const string PopupsPath = Path + "Popups/";
	public const string Tag = "UI";

    public const string ShowAnimation = "Show";
    public const string HideAnimation = "Hide";

	public static List<Popup> Shown = new List<Popup>();

    public event Action OnPopupShowing;
	public event Action OnPopupShown;
	public event Action OnPopupHiding;
    public event Action OnPopupHidden;

	public AudioClip ShowSound;
	public AudioClip HideSound;
	public PopupState State { get; set; }
	public bool IsInstance { get; set; }

	protected Animator animator;
	protected object data;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
		State = PopupState.Hidden;
		IsInstance = false;
    }

	public static Popup Show(string id, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = Popup.ShowAnimation)
	{
		return Show(id, GameObject.FindWithTag(Tag), data, onShown, mode, animation);
	}

	public static Popup Show(string id, GameObject parent, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = Popup.ShowAnimation)
	{
		return Show(id, parent?.transform, data, onShown, mode, animation);
	}

	public static Popup Show(string id, Transform parent, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = Popup.ShowAnimation)
	{
		return Show(id, PopupsPath, parent, data, onShown, mode, animation);
	}

	public static Popup Show(string id, string path, Transform parent, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = Popup.ShowAnimation)
	{
		if (mode != PopupShowMode.New)
		{
			Popup previous = Find(id);
			if (previous != null)
			{
				switch (mode)
				{
					case PopupShowMode.Dont:
						return null;

					case PopupShowMode.HidePrevious:
						previous.Hide();
						break;
					
					case PopupShowMode.Override:
						previous.Reshow(data, onShown);
						return previous;
				}
			}
		}

		string filePath = System.IO.Path.Combine(path, id);
		Popup prefab = Resources.Load<Popup>(filePath);
		Popup instance = Instantiate(prefab);
		instance.name = id;
		if (parent != null)
			instance.transform.SetParent(parent, false);
		instance.Show(data, onShown, animation);
		instance.IsInstance = true;
		return instance;
	}

	public static void Play(Transform from, AudioClip clip)
	{
		Transform root = from.root;
		AudioSource source = root.GetComponent<AudioSource>();
		if (source == null)
			source = root.gameObject.AddComponent<AudioSource>();
		source.PlayOneShot(clip);
	}

	public virtual void Play(AudioClip clip)
	{
		Play(transform, clip);
	}

    public virtual bool Show(object data = null, Action onShown = null, string animation = ShowAnimation)
    {
        if (!IsHidden())
            return false;

        State = PopupState.Showing;
        Data = data;
		Shown.Add(this);
		gameObject.SetActive(true);
		OnShowing();
		OnPopupShowing?.Invoke();
		bool animated = false;
		if (animator != null && animation != null)
        {
			int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
				animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => {
					OnShown(onShown);
				});
            }
        }
		Play(ShowSound);
		if (!animated)
			OnShown(onShown);
        return true;
    }

	public virtual void Reshow(object data = null, Action onShown = null)
	{
		Data = data;
		Play(ShowSound);
		onShown?.Invoke();
	}

	public static bool IsShown(string id)
	{
		return Find(id) != null;
	}

	public static Popup Find(string id)
	{
		Popup popup = Shown.Find(w => w.name == id);
			if (popup != null)
				return popup;
		return null;
	}

	public static bool Hide(string id, Action onHidden = null, PopupHideMode mode = PopupHideMode.Auto, string animation = Popup.HideAnimation)
	{
		Popup instance = Find(id);
		if (instance != null)
			return instance.Hide(onHidden, mode, animation);
		return false;
	}

	public virtual bool Hide(Action onHidden = null, PopupHideMode mode = PopupHideMode.Auto, string animation = HideAnimation)
    {
        if (!IsShown())
            return false;

        State = PopupState.Hiding;
        OnHiding();
		OnPopupHiding?.Invoke();
		bool animated = false;
		if (animator != null && animation != null)
        {
            int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
                animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => {
					OnHidden(onHidden, mode);
				});
            }
        }
		Play(HideSound);
        if (!animated)
			OnHidden(onHidden, mode);
        return true;
    }

	private void OnShown(Action onShown)
	{
		State = PopupState.Shown;
		OnShown();
		onShown?.Invoke();
		OnPopupShown?.Invoke();
	}

	private void OnHidden(Action onHidden, PopupHideMode mode)
	{
		data = null;
		State = PopupState.Hidden;
		if (mode == PopupHideMode.Destroy || (mode == PopupHideMode.Auto && IsInstance))
			Destroy(gameObject);
		else
			gameObject.SetActive(false);
		Shown.Remove(this);
		OnHidden();
		onHidden?.Invoke();
		OnPopupHidden?.Invoke();
	}

	protected virtual void OnDestroy()
	{
		Shown.Remove(this);
	}

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

    public virtual bool IsBusy()
    {
        return State == PopupState.Showing || State == PopupState.Hiding;
    }

    public virtual bool IsShown()
    {
        return State == PopupState.Shown;
    }

    public virtual bool IsHidden()
    {
        return State == PopupState.Hidden;
    }
}