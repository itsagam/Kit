using System.Collections.Generic;
using System.Linq;
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

		protected readonly Queue<SplashTask> tasks = new Queue<SplashTask>();

		#region Initalization
		protected void Awake()
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

		protected async UniTask RunTasks()
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

		protected UniTask LoadNextScene()
		{
			return SceneDirector.LoadScene(NextScene.Path);
		}
		#endregion

		#region Tasks
		protected void QueueTasks()
		{
			QueueTask("Task 1", TestTask(), 2);
			QueueTask("Task 2", TestTask(), 3);
			QueueTask("Task 4", TestTask(), 5);
			QueueTask("Task 5", TestTask(), 5);
			QueueTask("Task 6", TestTask(), 10);
		}

		protected static UniTask TestTask()
		{
			return UniTask.Delay(1000);
		}
		#endregion
	}
}