using Cysharp.Threading.Tasks;
using Kit;
using Kit.Modding;
using UnityEngine;
using UnityEngine.UI;

namespace Demos.Debug
{
	public class ModdingDemo: MonoBehaviour
	{
		public RawImage DisplayImage;

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
	}
}