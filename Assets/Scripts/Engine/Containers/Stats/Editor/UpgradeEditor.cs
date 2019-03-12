using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Engine.Containers.Editor
{
	public class UpgradeEditor
	{
		public class UpgradeDrawer : OdinValueDrawer<Upgrade>
		{
			protected override void DrawPropertyLayout(GUIContent label)
			{
				SirenixEditorGUI.BeginToolbarBox(label);
				CallNextDrawer(null);
				SirenixEditorGUI.EndToolbarBox();
			}
		}
	}
}