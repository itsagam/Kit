#if MODDING
using System.Collections.Generic;

namespace Modding
{
	public enum ModType
	{
		Patch,
		Mod
	}

	public struct ModGroup
	{
		public ModType Name;
		public string Path;
		public List<Mod> Mods;
		public bool Deactivatable;
		public bool Reorderable;

		public ModGroup(ModType name, string path, bool deactivatable = true, bool reorderable = true)
		{
			Name = name;
			Path = path;
			Mods = new List<Mod>();
			Deactivatable = deactivatable;
			Reorderable = reorderable;
		}
	}
}
#endif