namespace Engine.UI.Buttons
{
	public class WindowButton : ButtonBehaviour
	{
		public enum ShowHideMode
		{
			Show,
			Hide,
			Toggle
		}

		public Window Window;
		public ShowHideMode Action;

		protected override void OnClick()
		{
			ShowHide(Action);
		}

		private void ShowHide(ShowHideMode action)
		{
			if (Window == null)
				return;

			switch (action)
			{
				case ShowHideMode.Show:
					if (Window.gameObject.IsPrefab())
						UIManager.Show(Window);
					else
						Window.Show();
					break;

				case ShowHideMode.Hide:
					Window.Hide();
					break;

				case ShowHideMode.Toggle:
					ShowHide(Window.IsShown ? ShowHideMode.Hide : ShowHideMode.Show);
					break;
			}
		}
	}
}