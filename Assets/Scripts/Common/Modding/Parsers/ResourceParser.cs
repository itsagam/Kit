﻿using UnityEngine;
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
		public abstract List<Type> SupportedTypes { get; }
		public abstract List<string> SupportedExtensions { get; }
		public abstract OperateType OperateWith { get; }

		public abstract object Read<T>(object data, string path = null);

		public virtual float CanRead<T>(string path)
		{
			float certainty = 0;

			if (SupportedTypes.Any(t => typeof(T).IsAssignableFrom(t)))
				certainty += 0.5f;

			if (SupportedExtensions.Any(e => string.Compare(Path.GetExtension(path), e) == 0))
				certainty += 0.5f;

			return certainty;
		}

		public virtual object Write(object data, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual void Merge<T>(T current, object overwrite)
		{
			throw new NotImplementedException();
		}
	}
}