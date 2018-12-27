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
		public abstract ReadType ReadWith { get; }

		public virtual T Read<T>(byte[] data)
		{
			throw new NotImplementedException();
		}

		public virtual T Read<T>(string data)
		{
			throw new NotImplementedException();
		}

		public virtual bool CanRead(string fileName)
		{
			return SupportedExtensions.Any(e => string.Compare(Path.GetExtension(fileName), e) == 0);
		}
	}
}