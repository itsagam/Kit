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
		public abstract OperateType OperateWith { get; }
		public abstract bool CanRead<T>(string path);

		public virtual object Read<T>(object data, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual object Write(object obj, string path = null)
		{
			throw new NotImplementedException();
		}

		public virtual void Merge<T>(T a, T b)
		{
			throw new NotImplementedException();
		}
	}
}