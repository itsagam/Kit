using Cysharp.Threading.Tasks;
using Kit;
using Kit.Modding;
using Kit.UI.Message;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Demos.Modding
{
	[Hotfix]
	[LuaCallCSharp]
	public class Demo: MonoBehaviour
	{
		public RawImage DisplayImage;
		public MessageWindow MsgWindow;

		public void LoadMods()
		{
			ModManager.LoadModsAsync(true).Forget();
		}

		public void LoadResource()
		{
			Texture texture = ResourceManager.Load<Texture>(ResourceFolder.Resources, "Test.jpg");
			if (texture != null)
			{
				DisplayImage.texture = texture;
				DisplayImage.color = Color.white;
			}
		}

		public void InjectedReplace()
		{
			string message = @"
Steps to follow before testing injection in Editor:
1. Add HOTFIX_ENABLE in Scripting Define Symbols.
2. Press XLua -> Generate Code.
3. Press XLua -> Hotfix Inject in Editor.";
			MessageWindow.Show(MsgWindow, "Demo", message);
		}

		public void InjectedExtend()
		{
			Debugger.Log("Game code");
		}
	}
}