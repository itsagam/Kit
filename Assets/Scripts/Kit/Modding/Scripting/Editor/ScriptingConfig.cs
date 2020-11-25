#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using XLua;

namespace Kit.Modding.Scripting
{
    /// <summary>Configuration file for XLua.</summary>
    public static class ScriptingConfig
    {
        // Recommended that all types to be accessed in Lua have LuaCallCSharp or ReflectionUse.

        /// <summary>Set the proper path for XLua generated code.</summary>
        [CSObjectWrapEditor.GenPath]
        public static string GenPath
        {
            get
            {
                // If hotfixing/injection is enabled, all XLua code (including generated) needs to be in the main assembly, so
                // moving the code there as placing it in Plugins compiles it to Assembly-CSharp-firstpass.
#if !HOTFIX_ENABLE
                return "Assets/Plugins/XLua/Gen";
#else
                return "Assets/XLua/Gen";
#endif
            }
        }

		// /// <summary>Generate adapter code for these types, otherwise use reflection with lower performance.</summary>
		// [LuaCallCSharp]
		// public static IEnumerable<Type> LuaCallCSharp
		// {
		// 	get
		// 	{
		// 	}
		// }

		/// <summary>Allows you to adapt a Lua function to a C# delegate or to adapt a Lua table to a C# interface.</summary>
		[CSharpCallLua]
		public static IEnumerable<Type> CSharpCallLua
		{
			get
			{
				yield return typeof(IEnumerator);
			}
		}

		// /// <summary>Types and individual members marked with [Hotfix] can be injected. Caution.</summary>
		// [Hotfix]
		// public static IEnumerable<Type> Hotfix
		// {
		// 	get
		// 	{
		// 	}
		// }

		// /// <summary>Force reflection access on these types (and generate "link.xml" to block code stripping on IL2CPP).</summary>
		// [ReflectionUse]
		// public static IEnumerable<Type> ReflectionUse
		// {
		// 	get
		// 	{
		// 	}
		// }

		// /// <summary>Generate optimized code with no GC allocs for pure value-types.</summary>
		// [GCOptimize]
		// public static IEnumerable<Type> CSharpCallLua
		// {
		// 	get
		// 	{
		// 	}
		// }

		// /// <summary>If you do not want to generate adaption code for a member of a type, implement it with this attribute.</summary>
		// [BlackList]
		// public static List<List<string>> BlackList = new List<List<string>>()
		// 											 {
		// 											 };

		// /// <summary>
		// ///     Individual functions, fields, and properties marked with [DoNotGen] do not generate code and are accessed through
		// ///     reflection.
		// /// </summary>
		// [DoNotGen]
	}
}
#endif