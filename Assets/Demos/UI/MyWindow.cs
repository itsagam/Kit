
using Kit;
using Kit.UI;
using UnityEngine.UI;

namespace Demos.UI
{
	public class MyWindow : Window
	{
		public Text DataText;

		public override void Refresh()
		{
			DataText.text = (string) Data;
		}

		protected override void OnShowing()
		{
			Debugger.Log($"Showing {name}");
		}

		protected override void OnShown()
		{
			Debugger.Log($"{name} Shown");
		}

		protected override void OnHiding()
		{
			Debugger.Log($"Hiding {name}");
		}

		protected override void OnHidden()
		{
			Debugger.Log($"{name} Hidden");
		}
	}
}
