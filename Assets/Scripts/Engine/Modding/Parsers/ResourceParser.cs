using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Modding.Parsers
{
	public enum OperateType
	{
		Bytes,
		Text
	}

	public abstract class ResourceParser
	{
		public abstract IEnumerable<Type> SupportedReadTypes { get; }
		public virtual IEnumerable<Type> SupportedWriteTypes => Enumerable.Empty<Type>();
		public abstract IEnumerable<string> SupportedExtensions { get; }
		public abstract OperateType OperateWith { get; }

		public abstract T Read<T>(object data, string path = null) where T : class;

		public virtual float CanRead(string path, Type type)
		{
			float certainty = 0;

			if (SupportedReadTypes.Any(t => type.IsAssignableFrom(t)))
				certainty += 0.5f;

			if (SupportedExtensions.Any(e => string.Compare(Path.GetExtension(path), e) == 0))
				certainty += 0.5f;

			return certainty;
		}

		public virtual float CanWrite(string path, Type type)
		{
			float certainty = 0;

			if (SupportedWriteTypes.Any(t => type.IsAssignableFrom(t)))
				certainty += 0.5f;

			if (SupportedExtensions.Any(e => string.Compare(Path.GetExtension(path), e) == 0))
				certainty += 0.5f;

			return certainty;
		}

		public virtual T Write<T>(object data, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual void Merge(object current, object overwrite)
		{
			throw new NotImplementedException();
		}
	}
}