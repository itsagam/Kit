using System.Collections.Generic;
using System.Linq;
using Engine.Modding;
using Engine.UI.Widgets;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Splash
{
	public class SplashWindow : MonoBehaviour
	{
		public Text MessageText;
		public Image ProgressImage;
		public SceneReference NextScene;

		private readonly Queue<SplashTask> tasks = new Queue<SplashTask>();

		#region Initalization
		private void Awake()
		{
			SceneDirector.FadeIn();
		}
		#endregion

		#region Task execution
		private void Start()
		{
			QueueTasks();
			RunTasks().Forget();
		}

		public void QueueTask(string taskName, UniTask task, int taskWeight)
		{
			QueueTask(new SplashTask(taskName, task, taskWeight));
		}

		public void QueueTask(SplashTask task)
		{
			tasks.Enqueue(task);
		}

		private async UniTask RunTasks()
		{
			float totalWeight = 0;
			if (ProgressImage != null)
			{
				totalWeight = tasks.Sum(t => t.Weight);
				ProgressImage.fillAmount = 0;
			}

			while (tasks.Count > 0)
			{
				SplashTask task = tasks.Dequeue();
				if (MessageText != null)
					MessageText.text = task.Name;

				await task.Task;

				if (ProgressImage != null)
					ProgressImage.fillAmount += task.Weight / totalWeight;
			}

			await LoadNextScene();
		}

		private UniTask LoadNextScene()
		{
			return SceneDirector.LoadScene(NextScene.Path);
		}
		#endregion

		#region Tasks
		private void QueueTasks()
		{
			QueueModTasks();
		}

		private void QueueModTasks()
		{
#if MODDING
			var modPaths = ModManager.GetModPathsByGroup();
			int totalMods = modPaths.Sum(kvp => kvp.Value.Length);
			if (totalMods <= 0)
				return;

			QueueTask("Loading mods",   ModManager.LoadModsAsync(modPaths), totalMods);
			QueueTask("Executing mods", ModManager.ExecuteScriptsAsync(),   totalMods);
#endif
		}
		#endregion
	}
}