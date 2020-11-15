using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kit.UI
{
	/// <summary>
	/// A UI element that provides the cursor functionality for drag-and-drop operations.
	/// </summary>
	public class DragCursor: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		/// <summary>
		/// The <see cref="Item"/> for which this cursor is created.
		/// </summary>
		[Tooltip("The Item for which this cursor is created.")]
		public Item Item;

		/// <summary>
		/// How fast does the cursor move back to its initial position?
		/// </summary>
		[Tooltip("How fast does the cursor move back to its initial position?")]
		public float MoveSpeed = 750.0f;

		protected new Transform transform;
		protected Graphic graphic;
		protected Canvas canvas;

		protected Transform previousParent = null;
		protected Vector3 previousLocalPosition;
		protected Vector3 previousPosition;
		protected int previousIndex;

		protected virtual void Awake()
		{
			transform = base.transform;
			graphic = GetComponent<Graphic>();
			canvas = graphic.canvas;
		}

		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			graphic.raycastTarget = false;
			previousParent = transform.parent;
			previousLocalPosition = transform.localPosition;
			previousPosition = transform.position;
			previousIndex = transform.GetSiblingIndex();
			transform.SetParent(canvas.transform, true);
			transform.SetAsLastSibling();
			OnDrag(eventData);
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			transform.position = canvas.IsScreenSpace() ? (Vector3) eventData.position : ToWorld(eventData.position);
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			float speed = canvas.IsScreenSpace() ? MoveSpeed : MoveSpeed * canvas.transform.localScale.Min() / canvas.scaleFactor;
			transform.DOMove(previousPosition, speed).SetSpeedBased().OnComplete(PutBack);
		}

		protected virtual void PutBack()
		{
			transform.SetParent(previousParent, true);
			transform.localPosition = previousLocalPosition;
			transform.SetSiblingIndex(previousIndex);
			graphic.raycastTarget = true;
		}

		public virtual Vector3 ToWorld(Vector2 position)
		{
			return Camera.main.ScreenToWorldPoint(position).SetZ(((Component) this).transform.position.z);
		}
	}
}