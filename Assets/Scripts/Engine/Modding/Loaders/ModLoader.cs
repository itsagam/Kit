#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;

namespace Modding
{
    public abstract class ModLoader
	{
		public abstract Mod LoadMod(string path);
		public abstract UniTask<Mod> LoadModAsync(string path);
	}
}
#endif