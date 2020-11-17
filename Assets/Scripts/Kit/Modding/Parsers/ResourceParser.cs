using System;
using System.Linq;

namespace Kit.Parsers
{
	/// <summary>The mode used while reading/writing files.</summary>
	public enum ParseMode
	{
		/// <summary>Binary mode.</summary>
		Binary,

		/// <summary>Text mode.</summary>
		Text
	}

	/// <summary>Base class for file parsers.</summary>
	public abstract class ResourceParser
	{
		/// <summary>Return an array of types this parser can work with – output for reading, input for saving.</summary>
		public abstract Type[] SupportedTypes { get; }

		/// <summary>Return an array file extensions this parser can parse.</summary>
		public abstract string[] SupportedExtensions { get; }

		/// <summary>Return whether the parser works in text-mode or binary.</summary>
		public abstract ParseMode ParseMode { get; }

		/// <summary>Determines whether this parser can parse the specified type and path.</summary>
		/// <param name="type">The type of the object to parse.</param>
		/// <param name="path">The path to the file to parse.</param>
		/// <returns>The certainty with 0 being impossible and 1 being certain.</returns>
		public virtual float CanParse(Type type, string path)
		{
			float certainty = 0;

			if (SupportedTypes.Any(type.IsAssignableFrom))
				certainty += 0.5f;

			if (ResourceManager.MatchExtension(path, SupportedExtensions))
				certainty += 0.5f;

			return certainty;
		}

		/// <summary>Parse data for writing.</summary>
		/// <param name="data">The input object.</param>
		/// <param name="path">Path to the file.</param>
		/// <returns>String or byte-array.</returns>
		/// <exception cref="NotImplementedException">The parser does not support writing.</exception>
		public virtual object Write(object data, string path = null)
		{
			throw new NotImplementedException();
		}

		/// <summary>Parse data for reading.</summary>
		/// <param name="type">Type of object expected.</param>
		/// <param name="data">The input string or byte-array.</param>
		/// <param name="path">Path to the file.</param>
		/// <returns>The output object.</returns>
		/// <exception cref="NotImplementedException">The parser does not support reading.</exception>
		public virtual object Read(Type type, object data, string path = null)
		{
			throw new NotImplementedException();
		}

		/// <summary>Merge the contents of an object into the contents of another object.</summary>
		/// <param name="current">The object to merge into.</param>
		/// <param name="overwrite">The object to merge from.</param>
		/// <exception cref="NotImplementedException">The parser does not support merging.</exception>
		public virtual void Merge(object current, object overwrite)
		{
			throw new NotImplementedException();
		}
	}
}