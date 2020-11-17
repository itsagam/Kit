#if MODDING
using Cysharp.Threading.Tasks;

namespace Kit.Modding
{
	/// <summary>
	///     Template for classes that want to implement mod-loading from different types of packages. Each would need to implement
	///     its corresponding <see cref="Mod" /> sub-class.
	/// </summary>
	public abstract class ModLoader
	{
		/// <summary>Filename for the metadata file.</summary>
		public const string MetadataFile = "Metadata.json";

		/// <summary>Load a mod.</summary>
		/// <param name="path">Path to a mod.</param>
		/// <returns>Reference to the <see cref="Mod"/>, or <see langword="null"/> if it could not be loaded.</returns>
		public abstract Mod LoadMod(string path);

		/// <inheritdoc cref="LoadMod(string)"/>
		/// <summary>Load a mod asynchronously.</summary>
		public abstract UniTask<Mod> LoadModAsync(string path);
	}
}
#endif