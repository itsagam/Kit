using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;

public enum PopupState
{
    Shown,
    Showing,
    Hidden,
    Hiding
}

public enum PopupConflictMode
{
	ShowNew,
	DontShow,
	OverwriteData,
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
	public const string DefaultShowAnimation = "Show";
    public const string DefaultHideAnimation = "Hide";

	public static event Action<Popup> OnAnyPopupShowing;
	public static event Action<Popup> OnAnyPopupShown;
	public static event Action<Popup> OnAnyPopupHiding;
	public static event Action<Popup> OnAnyPopupHidden;

	public static List<Popup> Shown = new List<Popup>();

	public event Action OnPopupShowing;
	public event Action OnPopupShown;
	public event Action OnPopupHiding;
    public event Action OnPopupHidden;

	public AudioClip ShowSound;
	public AudioClip HideSound;
	public PopupState State { get; set; } = PopupState.Hidden;

	protected Animator animator;
	protected object data;
	protected bool isInstance = false;

	#region Functionality
	protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
    }

	public static Popup Show(string path, Transform parent, object data = null, Action onShown = null, PopupConflictMode mode = PopupConflictMode.HidePrevious, string animation = DefaultShowAnimation)
	{
		string name = Path.GetFileName(path);
		if (mode != PopupConflictMode.ShowNew)
		{
			Popup previous = Find(name);
			if (previous != null)
			{
				switch (mode)
				{
					case PopupConflictMode.DontShow:
						return null;

					case PopupConflictMode.HidePrevious:
						previous.Hide();
						break;
					
					case PopupConflictMode.OverwriteData:
						previous.Reshow(data, onShown);
						return previous;
				}
			}
		}

		Popup prefab = Resources.Load<Popup>(path);
		Popup instance = Instantiate(prefab);
		instance.name = prefab.name;
		if (parent != null)
			instance.transform.SetParent(parent, false);
		instance.Show(data, onShown, animation);
		instance.MarkAsInstance();
		return instance;
	}

    public virtual bool Show(object data = null, Action onShown = null, string animation = DefaultShowAnimation)
    {
        if (IsShown())
            return false;

        State = PopupState.Showing;
        Data = data;
		Shown.Add(this);
		gameObject.SetActive(true);
		OnShowing();
		OnPopupShowing?.Invoke();
		OnAnyPopupShowing?.Invoke(this);
		bool animated = false;
		if (animator != null && animation != null)
        {
			int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
				animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => OnShown(onShown));
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

	public virtual bool Hide(Action onHidden = null, PopupHideMode mode = PopupHideMode.Auto, string animation = DefaultHideAnimation)
    {
        if (IsHidden())
            return false;

        State = PopupState.Hiding;
        OnHiding();
		OnPopupHiding?.Invoke();
		OnAnyPopupHiding?.Invoke(this);
		bool animated = false;
		if (animator != null && animation != null)
        {
            int animationHash = Animator.StringToHash(animation);
            if (animator.HasState(0, animationHash))
            {
                animator.Play(animationHash);
                animator.Update(0);
                animated = true;
				Observable.Timer(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length)).Subscribe(l => OnHidden(onHidden, mode));
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
		OnAnyPopupShown?.Invoke(this);
	}

	private void OnHidden(Action onHidden, PopupHideMode mode)
	{
		data = null;
		State = PopupState.Hidden;
		if (mode == PopupHideMode.Destroy || (mode == PopupHideMode.Auto && isInstance))
		{
			Destroy(gameObject);
		}
		else
		{
			gameObject.SetActive(false);
			Shown.Remove(this);
		}
		OnHidden();
		onHidden?.Invoke();
		OnPopupHidden?.Invoke();
		OnAnyPopupHidden?.Invoke(this);
	}

	public virtual void MarkAsInstance()
	{
		isInstance = true;
	}

	protected virtual void OnDestroy()
	{
		Shown.Remove(this);
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
	public virtual void Play(AudioClip clip)
	{
		Play(transform, clip);
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

	#region Static functions
	public static Popup Show(string path, object data = null, Action onShown = null, PopupConflictMode mode = PopupConflictMode.HidePrevious, string animation = DefaultShowAnimation)
	{
		return Show(path, FindObjectOfType<Canvas>()?.transform, data, onShown, mode, animation);
	}

	public static Popup Show(string path, GameObject parent, object data = null, Action onShown = null, PopupConflictMode mode = PopupConflictMode.HidePrevious, string animation = DefaultShowAnimation)
	{
		return Show(path, parent?.transform, data, onShown, mode, animation);
	}

	public static bool Hide(string name, Action onHidden = null, PopupHideMode mode = PopupHideMode.Auto, string animation = DefaultHideAnimation)
	{
		Popup instance = Find(name);
		if (instance != null)
			return instance.Hide(onHidden, mode, animation);
		return false;
	}

	public static void Play(Transform from, AudioClip clip)
	{
		Transform root = from.root;
		AudioSource source = root.GetComponent<AudioSource>();
		if (source == null)
			source = root.gameObject.AddComponent<AudioSource>();
		source.PlayOneShot(clip);
	}

	public static Popup Find(string name)
	{
		return Shown.Find(w => w.name == name);
	}

	public static bool IsShown(string name)
	{
		return Find(name) != null;
	}

	public static Popup First
	{
		get
		{
			return Shown.FirstOrDefault();
		}
	}

	public static Popup Last
	{
		get
		{
			return Shown.LastOrDefault();
		}
	}
	#endregion
}