﻿using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UI
{
	public class DragCursor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Icon Icon;
		public float MoveSpeed = 750.0f;

		protected Transform transformCached;
		protected Graphic graphic;
		protected Canvas canvas;

		protected Transform previousParent = null;
		protected Vector3 previousLocalPosition;
		protected Vector3 previousPosition;
		protected int previousIndex;

		protected virtual void Awake()
		{
			transformCached = GetComponent<Transform>();
			graphic = GetComponent<Graphic>();
			canvas = graphic.canvas;
		}

		public virtual void OnBeginDrag (PointerEventData eventData)
		{
			graphic.raycastTarget = false;
			previousParent = transform.parent;
			previousLocalPosition = transformCached.localPosition;
			previousPosition = transformCached.position;
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
			transform.DOMove(previousPosition, speed).SetSpeedBased().OnComplete( MoveBack);
		}

		public virtual void MoveBack()
		{
			transform.SetParent(previousParent, true);
			transformCached.localPosition = previousLocalPosition;
			transform.SetSiblingIndex(previousIndex);
			graphic.raycastTarget = true;
		}

		public virtual Vector3 ToWorld(Vector2 position)
		{
			return Camera.main.ScreenToWorldPoint(position).SetZ(transform.position.z);
		}
	}
}