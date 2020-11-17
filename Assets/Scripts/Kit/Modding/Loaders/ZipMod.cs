#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Kit.Modding.Loaders
{
	/// <summary>Class ZipModLoader. Implements the <see cref="Kit.Modding.ModLoader" /></summary>
	/// <seealso cref="Kit.Modding.ModLoader" />
	public class ZipModLoader: ModLoader
	{
		/// <summary>The supported extensions</summary>
		public readonly List<string> SupportedExtensions = new List<string> { ".zip" };

		/// <summary>Loads the mod.</summary>
		/// <param name="path">The path.</param>
		/// <returns>Mod.</returns>
		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			ZipArchive archive = null;
			try
			{
				archive = ZipFile.OpenRead(path);
				ZipMod mod = new ZipMod(archive);
				ModMetadata metadata = mod.Load<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}

				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				archive?.Dispose();
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}

		/// <summary>load mod as an asynchronous operation.</summary>
		/// <param name="path">The path.</param>
		/// <returns>UniTask&lt;Mod&gt;.</returns>
		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			ZipArchive archive = null;
			try
			{
				archive = ZipFile.OpenRead(path);
				ZipMod mod = new ZipMod(archive);
				ModMetadata metadata = await mod.LoadAsync<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}

				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				archive?.Dispose();
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}
	}

	/// <summary>A compressed archive loaded as a mod. Loaded with <see cref="ZipModLoader"/>.</summary>
	/// <seealso cref="Kit.Modding.Mod" />
	public class ZipMod: Mod
	{
		/// <summary>The mod archive.</summary>
		public ZipArchive Archive { get; }

		/// <summary>Create a new instance with the given archive.</summary>
		/// <param name="archive">The archive.</param>
		public ZipMod(ZipArchive archive)
		{
			Archive = archive;
		}

		/// <inheritdoc />
		public override bool Exists(string path)
		{
			return Archive.GetEntry(path) != null;
		}

		/// <inheritdoc />
		public override string ReadText(string path)
		{
			try
			{
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				using (TextReader text = new StreamReader(stream))
					return text.ReadToEnd();
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override async UniTask<string> ReadTextAsync(string path)
		{
			try
			{
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				using (TextReader text = new StreamReader(stream))
					return await text.ReadToEndAsync();
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override byte[] ReadBytes(string path)
		{
			try
			{
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				{
					var data = new byte[entry.Length];
					stream.Read(data, 0, (int) entry.Length);
					return data;
				}
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			try
			{
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				{
					var data = new byte[entry.Length];
					await stream.ReadAsync(data, 0, (int) entry.Length);
					return data;
				}
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override IEnumerable<string> FindFiles(string path)
		{
			ZipArchiveEntry result = Archive.GetEntry(path);
			if (result != null)
				return EnumerableExtensions.One(path);

			if (Path.HasExtension(path))
				return Enumerable.Empty<string>();

			// Name is empty string for directory ZipArchiveEntries
			return Archive.Entries
						  .Where(entry => entry.Name != "" &&
										  ResourceManager.ComparePath(path, Path.ChangeExtension(entry.FullName, null)))
						  .Select(entry => entry.FullName);
		}

		/// <inheritdoc />
		public override void Unload()
		{
			base.Unload();
			Archive.Dispose();
		}
	}
}
#endif