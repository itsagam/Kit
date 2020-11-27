using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.UI.Buttons
{
	/// <summary>Button that shows, hides or toggles a window.</summary>
	public class WindowButton: ButtonBehaviour
	{
		/// <summary>What to do with the window?</summary>
		public enum ShowHideMode
		{
			/// <summary>Show the window.</summary>
			Show,

			/// <summary>Hide the window.</summary>
			Hide,

			/// <summary>Show if hidden and vice versa.</summary>
			Toggle
		}

		/// <summary>Whether to store just the path to the Window and load it at runtime using "Resources.Load".</summary>
		/// <remarks>
		///     If on, you cannot reference a Window from the scene and have to provide a prefab within a "Resources" folder. If off, the Window
		///     will be hard-referenced and be loaded with this button, whether it is opened or not.
		/// </remarks>
		[LabelText("Soft Reference")]
		[OnValueChanged("RefreshReference")]
		[Tooltip("Whether to store just the path to the Window and load it at runtime using \"Resources.Load\".\n\n"                   +
				 "If on, you cannot reference a Window from the scene and have to provide a prefab within a \"Resources\" folder.\n\n" +
				 "If off, the Window will be hard-referenced and be loaded with this button, whether it is opened or not.")]
		public bool UseSoftReference = true;

		/// <summary>Hard reference to the window.</summary>
		/// <remarks>Only kept if <see cref="UseSoftReference" /> is <see langword="false" />.</remarks>
		[LabelText("Window")]
		[HideIf("UseSoftReference")]
		[Tooltip("Hard reference to the window.")]
		public Window HardReference;

		/// <summary>A soft (path-only) reference to the window.</summary>
		/// <remarks>Only kept if <see cref="UseSoftReference" /> is <see langword="true" />.</remarks>
		[LabelText("Window")]
		[Tooltip("A soft (path-only) reference to the window.")]
		[ShowIf("UseSoftReference")]
		public WindowReference SoftReference;

		/// <summary>Whether to show, hide or toggle.</summary>
		/// <remarks>
		///     With a soft-reference, the file-name will be matched with a window to hide or toggle. With a hard-reference to an asset, the
		///     prefab name.
		/// </remarks>
		[Tooltip("Whether to show, hide or toggle. \n\n"                                                     +
				 "With a soft-reference, the file-name will be matched with a window to hide or toggle.\n\n" +
				 "With a hard-reference to an asset, the prefab name.")]
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
				UIManager.Show(SoftReference);
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
				SoftReference.SetAsset_Editor(HardReference != null && HardReference.IsPrefab() ? HardReference : null);
				HardReference = null;
			}
			else
			{
				HardReference = SoftReference.LoadAsset_Editor();
				SoftReference.SetAsset_Editor(null);
			}
		}
#endif
	}
}