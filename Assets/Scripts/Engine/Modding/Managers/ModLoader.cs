#if MODDING
using UniRx.Async;

namespace Engine.Modding
{
    public abstract class ModLoader
	{
		public const string MetadataFile = "Metadata.json";

		public abstract Mod LoadMod(string path);
		public abstract UniTask<Mod> LoadModAsync(string path);
	}
}
#endif