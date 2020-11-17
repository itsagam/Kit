using System;
using System.IO;
using UnityEngine;

namespace Kit.Parsers
{
	/// <summary>A texture parser.</summary>
	/// <seealso cref="Kit.Parsers.ResourceParser" />
	public class Texture2DParser: ResourceParser
	{
		/// <inheritdoc />
		public override Type[] SupportedTypes { get; } = { typeof(Texture2D) };

		/// <inheritdoc />
		public override string[] SupportedExtensions { get; } = { ".jpg", ".jpeg", ".png" };

		/// <inheritdoc />
		public override ParseMode ParseMode => ParseMode.Binary;

		/// <inheritdoc />
		public override object Read(Type type, object data, string path = null)
		{
			Texture2D texture = new Texture2D(0, 0);
			texture.LoadImage((byte[]) data);
			if (path != null)
				texture.name = path;
			return texture;
		}

		/// <inheritdoc />
		public override object Write(object data, string path = null)
		{
			Texture2D texture = (Texture2D) data;
			string extension = Path.GetExtension(path);
			switch (extension)
			{
				case ".png":
					return texture.EncodeToPNG();

				case ".exr":
					return texture.EncodeToEXR();

				case ".tga":
					return texture.EncodeToTGA();

				default:
					return texture.EncodeToJPG();
			}
		}
	}
}