﻿using System.IO;
using Engine.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.UI.Buttons
{
	public class WindowButton : ButtonBehaviour
	{
		public enum ShowHideMode
		{
			Show,
			Hide,
			Toggle
		}

#if UNITY_EDITOR
		[LabelText("Soft Reference")]
		[OnValueChanged("RefreshReference")]
		[Tooltip("Whether to store just the path to the Window and load it at runtime using \"Resources.Load\".\n\n" +
				 "If on, you cannot reference a Window from the scene and have to provide a prefab within a \"Resources\" folder.\n\n" +
				 "If off, the Window will be hard-referenced and be loaded with this button, whether it is opened or not.")]
#endif
		public bool UseSoftReference = true;

#if UNITY_EDITOR
		[LabelText("Window")]
		[HideIf("UseSoftReference")]
		[Tooltip("Hard reference to the window.")]
#endif
		public Window HardReference;

#if UNITY_EDITOR
		[LabelText("Window")]
		[Tooltip("Path to the window.")]
		[ShowIf("UseSoftReference")]
#endif
		public WindowReference SoftReference;

#if UNITY_EDITOR
		[Tooltip("Whether to show, hide or toggle. \n\n" +
				 "With a soft-reference, the filename will be matched to hide or toggle.\n\n" +
				 "With a hard-reference to an asset, the prefab name.")]
#endif
		public ShowHideMode Action;

		protected override void OnClick()
		{
			if (UseSoftReference  && SoftReference.Path.IsNullOrEmpty() ||
				!UseSoftReference && HardReference == null)
				return;

			switch (Action)
			{
				case ShowHideMode.Show:
					Show();
					break;

				case ShowHideMode.Hide:
					Hide();
					break;

				case ShowHideMode.Toggle:
					Toggle();
					break;
			}
		}

		protected void Show()
		{
			if (UseSoftReference)
			{
				UIManager.Show(SoftReference);
			}
			else
			{
				if (HardReference.IsPrefab())
					UIManager.Show(HardReference);
				else
					HardReference.Show();
			}
		}

		protected void Hide()
		{
			if (UseSoftReference)
			{
				string fileName = Path.GetFileNameWithoutExtension(SoftReference);
				UIManager.Hide(fileName);
			}
			else
			{
				if (HardReference.IsPrefab())
					UIManager.Hide(HardReference.name);
				else
					HardReference.Hide();
			}
		}

		protected void Toggle()
		{
			if (UseSoftReference)
			{
				string fileName = Path.GetFileNameWithoutExtension(SoftReference);
				if (UIManager.Find(fileName) == null)
					UIManager.Show(SoftReference);
				else
					UIManager.Hide(fileName);
			}
			else
			{
				if (HardReference.IsPrefab())
				{
					if (UIManager.Find(HardReference.name) == null)
						UIManager.Show(HardReference);
					else
						UIManager.Hide(HardReference.name);
				}
				else
				{
					if (HardReference.IsHidden)
						HardReference.Show();
					else
						HardReference.Hide();
				}
			}
		}

#if UNITY_EDITOR
		protected void RefreshReference()
		{
			if (UseSoftReference)
			{
				SoftReference.Asset = HardReference != null && HardReference.IsPrefab() ? HardReference : null;
				HardReference = null;
			}
			else
			{
				HardReference = SoftReference.Asset;
				SoftReference.Asset = null;
			}
		}
#endif
	}
}