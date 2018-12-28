using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Modding.Parsers
{
	public abstract class ByNameParser: ResourceParser
	{
		public abstract List<string> SupportedExtensions { get; }

		public override bool CanRead<T>(string path)
		{
			return SupportedExtensions.Any(e => string.Compare(Path.GetExtension(path), e) == 0);
		}
	}
}