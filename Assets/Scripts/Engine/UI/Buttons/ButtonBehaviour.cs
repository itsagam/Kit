using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
	public abstract class ButtonBehaviour : MonoBehaviour, IPointerClickHandler
	{
		protected abstract void OnClick();
		protected Button button;

		protected virtual void Awake()
		{
			button = GetComponent<Button>();
			if (button != null)
				button.onClick.AddListener(OnClick);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (button == null)
				OnClick();
		}
	}
}