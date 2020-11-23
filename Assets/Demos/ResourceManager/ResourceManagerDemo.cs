using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Kit;
using Kit.UI;
using UnityEngine;
using UnityEngine.UI;
using Debugger = Kit.Debugger;

namespace Demos.Debug
{
	public class ResourceManagerDemo: MonoBehaviour
	{
		public RawImage BackgroundImage;

		public void LoadFromResources()
		{
			TextAsset asset = ResourceManager.Load<TextAsset>(ResourceFolder.Resources, "Lua/General.lua.txt");
			Debugger.Log($"Size of the text is {asset.text.Length} characters.");
		}

		public void LoadFromExternalData()
		{
			Texture asset = ResourceManager.Load<Texture>(ResourceFolder.Data, "../Documentation/media/Console.png");
			BackgroundImage.texture = asset;
		}

		public void LoadAsync()
		{
			LoadWindow().Forget();
		}

		private async UniTask LoadWindow()
		{
			Window window = await ResourceManager.LoadAsync<Window>(ResourceFolder.Resources, "Windows/SettingsWindow");
			await UIManager.Show(window);
			Debugger.Log($"Window {window.name} has finished showing.");
		}

		public class Packages
		{
			public Dictionary<string, string> dependencies;
		}

		public void LoadJson()
		{
			Packages packages = ResourceManager.Load<Packages>(ResourceFolder.Data, "../Packages/manifest.json");
			Debugger.Log($"Project Dependency Count (including built-in packages): {packages.dependencies.Count}");
		}

		public void SaveObject()
		{
			string file = "MyObject.json";
			string path = ResourceManager.GetPath(ResourceFolder.PersistentData, file);
			bool result = ResourceManager.Save(path, ResourceManager.Paths);
			Debugger.Log($"{(result ? "Successfully saved to " : "Failed to saved to ")} {path}");
		}

		public void SaveAsync()
		{
			SaveTexture().Forget();
		}

		private async UniTask SaveTexture()
		{
			int width = 2048, height = width;
			int blocks = 8, blockSize = width / blocks;

			Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
					texture.SetPixel(x, y, (x / blockSize % 2 == 0) ^ (y / blockSize % 2 == 0) ? Color.white : Color.black);

			string file = "MyTexture.jpg";
			string path = ResourceManager.GetPath(ResourceFolder.PersistentData, file);
			bool result = await ResourceManager.SaveAsync(ResourceFolder.PersistentData, file, texture);

			Debugger.Log($"{(result ? "Successfully saved to " : "Failed to saved to ")} {path}");
			Process.Start(path);
		}

		public void ReadFile()
		{
			string data = ResourceManager.ReadText(ResourceFolder.Data, "../Packages/manifest.json");
			Debugger.Log(data ?? "File could not be read");
		}

		public void FileExists()
		{
			string file = "MyTexture.jpg";
			bool exists = ResourceManager.Exists(ResourceFolder.PersistentData, file);
			Debugger.Log($"File {file} {(exists ? " exists" : "does not exist")} in {ResourceFolder.PersistentData}");
		}

		public void DeleteFile()
		{
			string file = "MyTexture.jpg";
			bool result = ResourceManager.Delete(ResourceFolder.PersistentData, file);
			Debugger.Log($"File {file} {(result ? "didn't exist or was successfully deleted" : "could not be deleted")} from {ResourceFolder.PersistentData}");
		}
	}
}