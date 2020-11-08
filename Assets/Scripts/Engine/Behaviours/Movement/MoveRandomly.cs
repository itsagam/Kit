using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.Behaviours
{
    /// <summary>
    /// Keeping moving a transform towards a random position in a given area.
    /// </summary>
    public class MoveRandomly : MonoBehaviour
    {
        /// <summary>
        /// The area to move around in.
        /// </summary>
        /// <remarks></remarks>
        [Tooltip("The area to move around in; could be a collider, renderer or transform.")]
        public Component Area;



       // public bool Axis;

        /// <summary>
        /// Move around in the X-axis?
        /// </summary>
        [Tooltip("Move around in the X-axis?")]
        public bool X = true;

        /// <summary>
        /// Move around in the Y-axis?
        /// </summary>
        [Tooltip("Move around in the Y-axis?")]
        public bool Y = false;

        /// <summary>
        /// Move around in the Z-axis?
        /// </summary>
        [Tooltip("Move around in the Z-axis?")]
        public bool Z = true;

        /// <summary>
        /// The speed at which to move.
        /// </summary>
        [Tooltip("The speed at which to move.")]
        public float Speed = 5.0f;

        /// <summary>
        /// The easing to apply for the each cycle.
        /// </summary>
        [Tooltip("The easing to apply for each cycle.")]
        public Ease Easing = Ease.Linear;

        protected Bounds bounds;
        protected new Transform transform;

        protected void Awake()
        {
            if (Area == null)
                return;

            bounds = Area.GetBounds();
            transform = base.transform;
            Move();
        }

        protected void Move()
        {
            Vector3 destination = bounds.GetRandomPoint();
            Vector3 position = transform.position;
            if (!X)
                destination.x = position.x;
            if (!Y)
                destination.y = position.y;
            if (!Z)
                destination.z = position.z;
            transform.DOMove(destination, Speed).SetSpeedBased().SetEase(Easing).OnComplete(Move);
        }
    }
}