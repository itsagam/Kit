using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Modding.Resource
{
	public abstract class ResourceReader
	{
		public abstract List<string> SupportedExtensions { get; }
		public abstract OperateType OperateWith { get; }

		public virtual T Read<T>(object data)
		{
			throw new NotImplementedException();
		}

		public virtual object Write(object obj)
		{
			throw new NotImplementedException();
		}

		public virtual void Merge<T>(T a, T b)
		{
			throw new NotImplementedException();
		}

		public virtual bool CanRead(string fileName)
		{
			return SupportedExtensions.Any(e => string.Compare(Path.GetExtension(fileName), e) == 0);
		}
	}
}