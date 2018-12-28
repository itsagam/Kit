using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Modding.Resource
{
	public enum OperateType
	{
		Bytes,
		Text
	}

	public abstract class ResourceLoader
	{
		public abstract OperateType OperateWith { get; }
		public abstract List<Type> SupportedTypes { get; }

		public virtual T Load<T>(string path, object data) where T: UnityEngine.Object
		{
			throw new NotImplementedException();
		}

		public virtual bool CanLoad(Type type)
		{
			return SupportedTypes.Any(t => type.IsAssignableFrom(t));
		}
	}
}