namespace Kit.UI.Buttons
{
	/// <summary>
	/// Button that close/hides the last window opened.
	/// </summary>
	public class CloseButton: ButtonBehaviour
	{
		protected override void OnClick()
		{
			if (UIManager.Last != null)
				UIManager.Last.Hide();
		}
	}
}