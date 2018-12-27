using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Modding.Resource
{
	public enum ReadType
	{
		Bytes,
		Text
	}

	public abstract class ResourceLoader
	{
		public abstract ReadType LoadWith { get; }
		public abstract List<Type> SupportedTypes { get; }

		public virtual T Load<T>(string path, byte[] data) where T: UnityEngine.Object
		{
			throw new NotImplementedException();
		}

		public virtual T Load<T>(string path, string data) where T : UnityEngine.Object
		{
			throw new NotImplementedException();
		}

		public virtual bool CanLoad(Type type)
		{
			return SupportedTypes.Any(t => type.IsAssignableFrom(t));
		}
	}
}