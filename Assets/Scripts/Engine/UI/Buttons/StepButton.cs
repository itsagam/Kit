using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
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
					if (Button != null)
						Button.interactable = !isEdgeCase;
					break;
				}
				case StepMode.Hide:
				{
					if (Button != null)
						Button.gameObject.SetActive(!isEdgeCase);
					break;
				}
			}
		}

		public bool IsEdgeCase => Direction == StepDirection.Previous && Wizard.Index <= 0 ||
								  Direction == StepDirection.Next     && Wizard.Index >= Wizard.Count - 1;

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
}