using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Move the <see cref="Transform"/> in a given direction.</summary>
	public class MoveInDirection: MonoBehaviour
	{
		/// <summary>Direction to move in.</summary>
		[ShowInInspector]
		[PropertyTooltip("Direction to move in.")]
		public Vector3 Direction
		{
			get => direction;
			set => direction = value.normalized;
		}

		/// <summary>The speed at which to move.</summary>
		[Tooltip("The speed at which to move.")]
		public float Speed = 5.0f;

		[SerializeField]
		[HideInInspector]
		protected Vector3 direction = Vector3.up;

		protected new Transform transform;

		protected void Awake()
		{
			transform = base.transform;
		}

		protected virtual void Update()
		{
			transform.Translate(direction * (Speed * Time.deltaTime));
		}
	}
}