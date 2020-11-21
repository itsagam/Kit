namespace Kit.Pooling
{
	/// <summary>Interface to be implemented by pool components which want to be informed of pooling events directly.</summary>
	public interface IPooled
	{
		/// <summary>Method that gets called when an instance initializes.</summary>
		void AwakeFromPool();

		/// <summary>Method that gets called when an instance gets pooled.</summary>
		void OnDestroyIntoPool();
	}
}