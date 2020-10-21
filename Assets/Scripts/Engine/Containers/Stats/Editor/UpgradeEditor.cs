using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Engine.Containers.Editor
{
	public class UpgradeDrawer: OdinValueDrawer<Upgrade>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginToolbarBox(label);
			CallNextDrawer(null);
			SirenixEditorGUI.EndToolbarBox();
		}
	}

	public class UpgradeListProcessor: OdinAttributeProcessor<UpgradeList>
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class UpgradeListDrawer: OdinValueDrawer<UpgradeList>
	{
		protected override void Initialize()
		{
			base.Initialize();
			if (ValueEntry.SmartValue == null)
				Property.Tree.DelayActionUntilRepaint(() => ValueEntry.SmartValue = new UpgradeList());
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
		}
	}
}