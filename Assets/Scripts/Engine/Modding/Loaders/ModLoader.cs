using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Modding
{
    public abstract class ModLoader
	{
		protected abstract Task<Mod> LoadModInternal(string path, bool async);

		// TODO: Cleanup and merge the loader code
		public virtual Mod LoadMod(string path)
		{
			return LoadModInternal(path, false).Result;
		}

		public virtual async Task<Mod> LoadModAsync(string path)
		{
			return await LoadModInternal(path, true);
		}
	}
}