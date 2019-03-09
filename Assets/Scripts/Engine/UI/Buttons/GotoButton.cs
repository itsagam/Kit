using UnityEngine;
using UnityEngine.EventSystems;

public class GotoButton : MonoBehaviour, IPointerClickHandler
{
	public Wizard Wizard;
	public Window Step;

	public void OnPointerClick (PointerEventData eventData)
	{
		Wizard?.GoTo(Step);
	}
}