using UnityEngine;

namespace Engine.UI
{
    public class ObjectList : IconList
    {
        public Object[] Objects;

        public virtual void Start()
        {
            if (Objects != null)
                Items = Objects;
        }
    }
}