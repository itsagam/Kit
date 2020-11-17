using System;
using UnityEngine;

namespace Kit.Parsers
{
	/// <summary>A <see cref="TextAsset" /> parser.</summary>
	/// <seealso cref="Kit.Parsers.ResourceParser" />
	public class TextAssetParser: ResourceParser
	{
		/// <inheritdoc />
		public override Type[] SupportedTypes { get; } = { typeof(TextAsset) };

		/// <inheritdoc />
		public override string[] SupportedExtensions { get; } = { ".txt" };

		/// <inheritdoc />
		public override ParseMode ParseMode => ParseMode.Text;

		/// <inheritdoc />
		public override object Read(Type type, object data, string path = null)
		{
			TextAsset asset = new TextAsset((string) data);
			if (path != null)
				asset.name = path;
			return asset;
		}

		/// <inheritdoc />
		public override object Write(object data, string path = null)
		{
			return ((TextAsset) data).text;
		}
	}
}