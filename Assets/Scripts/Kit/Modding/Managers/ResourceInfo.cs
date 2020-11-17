#if MODDING
using System;
using Kit.Parsers;

namespace Kit.Modding
{
	/// <summary>Information about a loaded resource.</summary>
	public struct ResourceInfo
	{
		/// <summary>Mod this resource was loaded with.</summary>
		public Mod Mod;

		/// <summary>Path to the resource.</summary>
		public string Path;

		/// <summary>The parser used to parse it.</summary>
		public ResourceParser Parser;

		/// <summary>Reference to the resource.</summary>
		public WeakReference Reference;

		/// <inheritdoc cref="ResourceInfo(Mod, string, ResourceParser, WeakReference)" />
		public ResourceInfo(Mod mod, string file, ResourceParser parser, object reference)
			: this(mod, file, parser, new WeakReference(reference))
		{
		}

		/// <summary>Create a new <see cref="ResourceInfo" /> with specified information.</summary>
		/// <param name="mod">The mod used to load it.</param>
		/// <param name="file">Path to the file.</param>
		/// <param name="parser">Parser used to parse it.</param>
		/// <param name="reference">Reference to the resource.</param>
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