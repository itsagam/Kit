#if MODDING
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kit;
using Kit.Modding;
using Kit.Parsers;
using Kit.UI.Message;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using Object = System.Object;


namespace Demos.Modding
{
	public class TestData
	{
		public string Title;
		public float Speed;
		public float Damage;
	}
#if HOTFIX_ENABLE
    [Hotfix]
#endif
    public class Demo: MonoBehaviour
	{
		public RawImage DisplayImage;
		public MessageWindow MsgWindow;
		public List<Button> DisabledButtons;

		public string Title { get; protected set; } = "Demo";

		protected void Awake()
		{
			DisabledButtons.ForEach(b => b.interactable = false);
		}

		public void LoadMods()
		{
			ModManager.LoadModsAsync(true).Forget();
			DisabledButtons.ForEach(b => b.interactable = true);
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

		public void LoadMerged()
		{
			TestData testData = ResourceManager.Load<TestData>(ResourceFolder.StreamingAssets, "Test.json", merge: true);
			if (testData != null)
			{
				Title = testData.Title;
				MessageWindow.Show(MsgWindow, Title, $"Speed: {testData.Speed}\nDamage: {testData.Damage}");
			}
		}

        public void ClickMe()
        {
            MessageWindow.Show(MsgWindow, Title, "Dang it!\nSee the lua scripts for how this is being done.");
        }

        public void InjectedReplace()
		{
			string message = @"
Steps to follow before testing injection in Editor:
1. Add HOTFIX_ENABLE in Scripting Define Symbols.
2. Press XLua -> Generate Code.
3. Press XLua -> Hotfix Inject in Editor.";
			MessageWindow.Show(MsgWindow, Title, message);
		}

		public void InjectedExtend()
		{
			Debugger.Log("Game code");
		}
	}
}
#endif