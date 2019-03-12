using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine.UI.Buttons
{
	public class GotoButton : MonoBehaviour, IPointerClickHandler
	{
		public Wizard Wizard;
		public Window Step;

		public void OnPointerClick (PointerEventData eventData)
		{
			if (Wizard != null)
				Wizard.GoTo(Step);
		}
	}
}