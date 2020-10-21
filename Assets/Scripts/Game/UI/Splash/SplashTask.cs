using Cysharp.Threading.Tasks;

namespace Game.UI.Splash
{
	public struct SplashTask
	{
		public string Name;
		public UniTask Task;
		public float Weight;

		public SplashTask(string name, UniTask task, int weight)
		{
			Name = name;
			Task = task;
			Weight = weight;
		}
	}
}