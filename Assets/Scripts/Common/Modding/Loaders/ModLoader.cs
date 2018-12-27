using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
    public abstract class ModLoader
	{
		public abstract ModPackage LoadMod(string path);
	}
}