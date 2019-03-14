using System.Collections.Generic;
using System.Linq;
using Engine;
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

		protected void Awake()
		{
			SceneDirector.FadeIn();
		}

		private void Start()
		{
			QueueTasks();
			RunTasks().Forget();
		}

		protected void QueueTasks()
		{
			QueueTask("Task 1", TestTask(), 2);
			// QueueTask("Task 2", TestTask(), 3);
			// QueueTask("Task 4", TestTask(), 5);
			// QueueTask("Task 5", TestTask(), 5);
			// QueueTask("Task 6", TestTask(), 10);
		}

		protected void QueueTask(string message, UniTask task, int weight)
		{
			QueueTask(new SplashTask(message, task, weight));
		}

		protected void QueueTask(SplashTask task)
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

			OnComplete();
		}


		protected void OnComplete()
		{
			SceneDirector.LoadScene(NextScene.Path);
		}

		protected UniTask TestTask()
		{
			return UniTask.Delay(1000);
		}
	}
}