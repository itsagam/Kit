using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
	public class CloseButton : MonoBehaviour, IPointerClickHandler
	{
		protected Button button;

		protected void Awake()
		{
			button = GetComponent<Button>();
			if (button != null)
				button.onClick.AddListener(Close);
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			if (button == null)
				Close();
		}

		protected static void Close()
		{
			if (UIManager.Last != null)
				UIManager.Last.Hide();
		}
	}
}