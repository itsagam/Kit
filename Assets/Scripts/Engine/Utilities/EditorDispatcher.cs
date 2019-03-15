// Use Sirenix.OdinInspector.Editor.UnityEditorEventUtility instead

// #if UNITY_EDITOR
// using System;
// using System.Collections.Generic;
// using UnityEditor;
//
// namespace Engine
// {
// 	public static class EditorDispatcher
// 	{
// 		private static Queue<Action> actions = new Queue<Action>();
//
// 		static EditorDispatcher()
// 		{
// 			EditorApplication.update += Update;
// 		}
//
// 		private static void Update()
// 		{
// 			while (actions.Count > 0)
// 			{
// 				Action action = actions.Dequeue();
// 				try
// 				{
// 					action();
// 				}
// 				catch (Exception ex)
// 				{
// 					Debugger.Log("EditorDispatcher", ex.Message);
// 				}
// 			}
// 		}
//
// 		public static void Enqueue(Action action)
// 		{
// 			actions.Enqueue(action);
// 		}
// 	}
// }
// #endif