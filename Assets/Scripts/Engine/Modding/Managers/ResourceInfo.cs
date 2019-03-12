#if MODDING
using System;
using Engine.Parsers;

namespace Engine.Modding
{
	public struct ResourceInfo
	{
		public Mod Mod;
		public string Path;
		public ResourceParser Parser;
		public WeakReference Reference;

		public ResourceInfo(Mod mod, string file, ResourceParser parser, object reference)
			: this(mod, file, parser, new WeakReference(reference))
		{
		}

		public ResourceInfo(Mod mod, string file, ResourceParser parser, WeakReference reference)
		{
			Mod = mod;
			Path = file;
			Parser = parser;
			Reference = reference;
		}
	}
}
#endif