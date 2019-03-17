using System;
using System.Linq;

namespace Engine.Parsers
{
	public enum ParseMode
	{
		Binary,
		Text
	}

	public abstract class ResourceParser
	{
		public abstract Type[] SupportedTypes { get; }
		public abstract string[] SupportedExtensions { get; }
		public abstract ParseMode ParseMode { get; }

		public virtual float CanParse(Type type, string path)
		{
			float certainty = 0;

			if (SupportedTypes.Any(type.IsAssignableFrom))
				certainty += 0.5f;

			if (ResourceManager.MatchExtension(path, SupportedExtensions))
				certainty += 0.5f;

			return certainty;
		}

		public virtual object Write(object data, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual object Read(Type type, object data, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual void Merge(object current, object overwrite)
		{
			throw new NotImplementedException();
		}
	}
}