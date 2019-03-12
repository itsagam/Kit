﻿#if MODDING
using UniRx.Async;

namespace Engine.Modding
{
    public abstract class ModLoader
	{
		public abstract Mod LoadMod(string path);
		public abstract UniTask<Mod> LoadModAsync(string path);
	}
}
#endif