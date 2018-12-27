using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding
{
	public abstract class ModParser
	{
		public enum ParseType
		{
			Bytes,
			Text
		}

		public abstract ParseType ParseWith { get; }
		public abstract List<Type> SupportedTypes { get; }

		public virtual T Parse<T>(string path, byte[] data) where T: UnityEngine.Object
		{
			throw new NotImplementedException();
		}

		public virtual T Parse<T>(string path, string data) where T : UnityEngine.Object
		{
			throw new NotImplementedException();
		}
	}
}