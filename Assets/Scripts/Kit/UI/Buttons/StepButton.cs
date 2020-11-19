using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kit.UI.Buttons
{
	/// <summary>Button that goes to the next/previous screen in a <see cref="Wizard" />.</summary>
	public class StepButton: ButtonBehaviour
	{
		/// <summary>The direction to move.</summary>
		public enum StepDirection
		{
			Next,
			Previous
		}

		/// <summary>What to do when there are no further screens?</summary>
		public enum StepMode
		{
			/// <summary>Do nothing. Keep the button as is.</summary>
			Nothing,

			/// <summary>Change the button's text.</summary>
			Change,

			/// <summary>Disable the button.</summary>
			Disable,

			/// <summary>Hide the button.</summary>
			Hide
		}

		/// <summary>The wizard this button interacts with.</summary>
		[Required]
		[Tooltip("The wizard this button interacts with.")]
		public Wizard Wizard;

		/// <summary>Direction to move.</summary>
		[Tooltip("Direction to move.")]
		public StepDirection Direction = StepDirection.Next;

		/// <summary>What to do when it is no longer possible to use the button?</summary>
		[Tooltip("What to do when it is no longer possible to use the button?")]
		public StepMode Mode = StepMode.Hide;

		/// <summary>Text-field to use when changing text.</summary>
		[Tooltip("Text-field to use when changing text.")]
		[ShowIf("Mode", StepMode.Change)]
		public Text Text;

		/// <summary>Text to change to.</summary>
		[Tooltip("Text to change to.")]
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

			Refresh();
			Wizard.Changing.AddListener(OnChanging);
		}

		protected void OnChanging(int previousIndex, Window previous, int nextIndex, Window next)
		{
			Refresh();
		}

		protected void Refresh()
		{
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

		/// <summary>Returns whether the button can no longer do something.</summary>
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