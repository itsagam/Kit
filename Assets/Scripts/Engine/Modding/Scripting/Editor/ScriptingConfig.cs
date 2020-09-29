#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using XLua;

namespace Engine.Modding.Scripting
{
	public static class ScriptingConfig
	{
		// Recommended that all types to be accessed in Lua have LuaCallCSharp or ReflectionUse

		/*
		// Generate adapter code for these types, otherwise use reflection with lower performance
		[LuaCallCSharp]
		public static IEnumerable<Type> LuaCallCSharp
		{
				get
				{
				}
		}
		*/

		// Allows you to adapt a Lua function to a C# delegate or to adapt a Lua table to a C# interface
		[CSharpCallLua]
		public static IEnumerable<Type> CSharpCallLua
		{
				get
				{
					yield return typeof(IEnumerator);
				}
		}

		/*
		[Hotfix]
		// Types and individual members marked with [Hotfix] can be injected. Caution.
		public static IEnumerable<Type> Hotfix
		{
			get
			{
			}
		}
		*/

		/*
		[ReflectionUse]
		// Force reflection access on these types (and generate "link.xml" to block code stripping on IL2CPP)
		public static IEnumerable<Type> ReflectionUse
		{
			get
			{
			}
		}
		*/

		/*
		// Generate optimized code with no gc allocs for pure value-types
		[GCOptimize]
		public static IEnumerable<Type> CSharpCallLua
		{
			get
			{
			}
		}
		*/

		/*
		// If you do not want to generate adaption code for a member of a type, implement it with this attribute
		[BlackList]
		public static List<List<string>> BlackList = new List<List<string>>()
		{

		};
		*/

		// Individual functions, fields, and properties marked with [DoNotGen] do not generate code and are accessed through reflection
		//[DoNotGen]
	}
}
#endif