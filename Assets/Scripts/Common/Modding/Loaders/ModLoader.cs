using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Modding
{
    public abstract class ModLoader
	{
		protected abstract Task<ModPackage> LoadModInternal(string path, bool async);

		public virtual ModPackage LoadMod(string path)
		{
			return LoadModInternal(path, false).Result;
		}

		public virtual async Task<ModPackage> LoadModAsync(string path)
		{
			return await LoadModInternal(path, true);
		}
	}
}