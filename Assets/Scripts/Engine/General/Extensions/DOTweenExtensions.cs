using System.Threading.Tasks;
using DG.Tweening;
using UniRx.Async;

namespace Engine
{
	public static class DOTweenExtensions
	{
		public static UniTask ToUniTask(this Tween tween)
		{
			bool isCompleted = false;
			tween.onComplete += () => isCompleted = true;
			return UniTask.WaitUntil(() => isCompleted);
		}

		public static Task<Tween> ToTask(this Tween tween)
		{
			var completionSource = new TaskCompletionSource<Tween>();
			tween.onComplete += () => completionSource.SetResult(tween);
			return completionSource.Task;
		}
	}
}