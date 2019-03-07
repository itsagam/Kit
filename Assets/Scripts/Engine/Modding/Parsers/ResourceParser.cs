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
		public abstract IEnumerable<Type> SupportedTypes { get; }
		public abstract IEnumerable<string> SupportedExtensions { get; }
		public abstract OperateType OperateWith { get; }

		public virtual float CanOperate(Type type, string path)
		{
			float certainty = 0;

			if (SupportedTypes.Any(t => type.IsAssignableFrom(t)))
				certainty += 0.5f;

			if (SupportedExtensions.Any(e => string.Compare(Path.GetExtension(path), e) == 0))
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