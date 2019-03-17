namespace Engine.UI.Buttons
{
	public class GotoButton : ButtonBehaviour
	{
		public Wizard Wizard;
		public Window Step;

		protected override void OnClick()
		{
			if (Wizard != null)
				Wizard.GoTo(Step);
		}
	}
}