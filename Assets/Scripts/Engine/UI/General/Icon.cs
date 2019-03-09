using UnityEngine;

public class Icon : MonoBehaviour
{
    protected object data;

    public virtual void Refresh()
    {
    }

    public virtual object Data
    {
        get => data;
        set
        {
            data = value;
            Refresh();
        }
    }

    public virtual int Index
    {
        get => transform.GetSiblingIndex();
        set => transform.SetSiblingIndex(value);
    }
}