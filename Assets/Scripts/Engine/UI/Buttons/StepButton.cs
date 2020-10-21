using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
	public class StepButton: ButtonBehaviour
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

		protected string originalText;

		protected override void Awake()
		{
			base.Awake();
			if (Mode == StepMode.Nothing)
				return;

			if (Text != null)
				originalText = Text.text;

			if (Wizard != null)
				Wizard.Changing.AddListener(OnChanging);
		}

		protected void OnChanging(int previousIndex, Window previous, int nextIndex, Window next)
		{
			if (Wizard == null)
				return;

			bool isEdgeCase = IsEdgeCase;
			switch (Mode)
			{
				case StepMode.Change:
				{
					if (Text != null && !Change.IsNullOrEmpty())
						Text.text = isEdgeCase ? Change : originalText;
					break;
				}
				case StepMode.Disable:
				{
					if (button != null)
						button.interactable = !isEdgeCase;
					break;
				}
				case StepMode.Hide:
				{
					gameObject.SetActive(!isEdgeCase);
					break;
				}
			}
		}

		public bool IsEdgeCase => Direction == StepDirection.Previous && Wizard.Index <= 0 ||
								  Direction == StepDirection.Next     && Wizard.Index >= Wizard.Count - 1;

		protected override void OnClick()
		{
			if (Wizard == null)
				return;

			if (Direction == StepDirection.Next)
				Wizard.Next();
			else
				Wizard.Previous();
		}
	}
}