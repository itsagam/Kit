using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
	public class PlayOnClick : MonoBehaviour, IPointerDownHandler
	{
		public AudioClip Audio;

		public void OnPointerDown (PointerEventData eventData)
		{
			Button button = GetComponent<Button>();
			if (button == null || button.IsInteractable())
				AudioManager.PlayUIEffect(Audio);
		}
	}
}