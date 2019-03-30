using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UI
{
	public class DragCursor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Icon Icon;
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

		public virtual void OnBeginDrag (PointerEventData eventData)
		{
			graphic.raycastTarget = false;
			previousParent = ((Component) this).transform.parent;
			previousLocalPosition = transform.localPosition;
			previousPosition = transform.position;
			previousIndex = ((Component) this).transform.GetSiblingIndex();
			((Component) this).transform.SetParent(canvas.transform, true);
			((Component) this).transform.SetAsLastSibling();
			OnDrag(eventData);
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			((Component) this).transform.position = canvas.IsScreenSpace() ? (Vector3) eventData.position : ToWorld(eventData.position);
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			float speed = canvas.IsScreenSpace() ? MoveSpeed : MoveSpeed * canvas.transform.localScale.Min() / canvas.scaleFactor;
			((Component) this).transform.DOMove(previousPosition, speed).SetSpeedBased().OnComplete( MoveBack);
		}

		public virtual void MoveBack()
		{
			((Component) this).transform.SetParent(previousParent, true);
			transform.localPosition = previousLocalPosition;
			((Component) this).transform.SetSiblingIndex(previousIndex);
			graphic.raycastTarget = true;
		}

		public virtual Vector3 ToWorld(Vector2 position)
		{
			return Camera.main.ScreenToWorldPoint(position).SetZ(((Component) this).transform.position.z);
		}
	}
}