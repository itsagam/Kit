namespace Engine.UI.Buttons
{
	public class CloseButton: ButtonBehaviour
	{
		protected override void OnClick()
		{
			if (UIManager.Last != null)
				UIManager.Last.Hide();
		}
	}
}