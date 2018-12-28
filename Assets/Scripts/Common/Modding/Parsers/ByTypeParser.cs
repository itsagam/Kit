using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Modding.Parsers
{
	public abstract class ByTypeParser: ResourceParser
	{
		public abstract List<Type> SupportedTypes { get; }

		public override bool CanRead<T>(string path)
		{
			return SupportedTypes.Any(t => typeof(T).IsAssignableFrom(t));
		}
	}
}