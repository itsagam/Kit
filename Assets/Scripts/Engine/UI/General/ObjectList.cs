public class ObjectList : IconList
{
    public UnityEngine.Object[] Objects;

    public virtual void Start()
    {
        if (Objects != null)
            Items = Objects;
    }
}