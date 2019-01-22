using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class StepButton : MonoBehaviour
{
	public enum StepDirection
	{
		Next,
		Previous
	}

	public enum StepMode
	{
		Nothing,
		Change,
		Disable,
		Hide
	}

	public Wizard Wizard;
	public Text Text;
	public StepDirection Direction = StepDirection.Next;
	public StepMode Mode = StepMode.Nothing;
	public string Change;
	public bool Hide = false;
	public Button Button { get; protected set; }

	protected string originalText;

	protected void Awake()
	{
		Button = GetComponent<Button>();
		Button.onClick.AddListener(Perform);

		if (Mode == StepMode.Nothing)
			return;
		
		if (Text != null)
			originalText = Text.text;

		if (Wizard != null)
		{
			Wizard.OnChange += OnChange;
			Wizard.OnPopupHiding += OnHide;
		}
	}

	protected void OnHide()
	{
		if (Hide || IsEdgeCase)
			Button.gameObject.SetActive(false);
	}

	protected void OnChange(int previousIndex, Popup previous, int nextIndex, Popup next)
	{
		if (Wizard == null)
			return;
		
		bool isEdgeCase = IsEdgeCase;
		if (isEdgeCase && !Button.gameObject.activeSelf)
			Button.gameObject.SetActive(true);
		if (Mode == StepMode.Change)
		{
			if (Text != null && !Change.IsNullOrEmpty())
				Text.text = isEdgeCase ? Change : originalText;
		}
		else if (Mode == StepMode.Disable)
		{
			if (Button != null)
				Button.interactable = !isEdgeCase;
		}
		else if (Mode == StepMode.Hide)
		{
			if (Button != null)
				Button.gameObject.SetActive(!isEdgeCase);
		}
	}

	public bool IsEdgeCase
	{
		get
		{
			return 
				(Direction == StepDirection.Previous && Wizard.Index <= 0) ||
				(Direction == StepDirection.Next && Wizard.Index >= Wizard.Count - 1);	
		}
	}

	public void Perform()
	{
		if (Wizard == null)
			return;
		
		if (Direction == StepDirection.Next)
			Wizard.Next();
		else
			Wizard.Previous();
	}
}