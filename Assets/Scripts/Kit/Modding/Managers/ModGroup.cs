#if MODDING
using System.Collections.Generic;

namespace Kit.Modding
{
	/// <summary>Type of a mod. Identifier for a <see cref="ModGroup" />.</summary>
	public enum ModType
	{
		/// <summary>A patch.</summary>
		Patch,

		/// <summary>A mod.</summary>
		Mod
	}

	/// <summary>Mod groups decide how mods are loaded.</summary>
	public class ModGroup
	{
		/// <summary>Group identifier.</summary>
		public ModType Name;

		/// <summary>Base path where the mods of this group will be searched for.</summary>
		public string Path;

		/// <summary>Whether mods of this group can be de-activated.</summary>
		public bool Deactivatable;

		/// <summary>Whether mods of this group can be re-ordered.</summary>
		public bool Reorderable;

		/// <summary>List of mods belonging to this group.</summary>
		public List<Mod> Mods = new List<Mod>();

		/// <summary>Create a new <see cref="ModGroup"/>.</summary>
		/// <param name="name">Group identifier.</param>
		/// <param name="path">>Base path where the mods of this group will be searched for.</param>
		/// <param name="deactivatable">Whether mods of this group can be de-activated.</param>
		/// <param name="reorderable">Whether mods of this group can be re-ordered.</param>
		public ModGroup(ModType name, string path, bool deactivatable = true, bool reorderable = true)
		{
			Name = name;
			Path = path;
			Deactivatable = deactivatable;
			Reorderable = reorderable;
		}
	}
}
#endif