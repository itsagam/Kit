using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

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

	[Tooltip("Wizard to move next/previous")]
	public Wizard Wizard;

	[Tooltip("Direction to move")]
	public StepDirection Direction = StepDirection.Next;

	[Tooltip("What to do when it is no longer possible to use the button")]
	public StepMode Mode = StepMode.Nothing;

	[Tooltip("Text-field to use when changing text")]
	[ShowIf("Mode", StepMode.Change)]
	public Text Text;

	[Tooltip("Text to change to")]
	[ShowIf("Mode", StepMode.Change)]
	public string Change;

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
			Wizard.OnChanging.AddListener(OnChanging);
	}

	protected void OnChanging(int previousIndex, Window previous, int nextIndex, Window next)
	{
		if (Wizard == null)
			return;
		
		bool isEdgeCase = IsEdgeCase;
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