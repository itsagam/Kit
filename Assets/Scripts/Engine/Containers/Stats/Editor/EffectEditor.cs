using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Engine.Containers.Editor
{
	public class EffectDrawer: OdinValueDrawer<Effect>
	{
		public const float EffectWidth = 120;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginIndentedHorizontal();
			Effect effect = ValueEntry.SmartValue;
			effect.Stat = SirenixEditorGUI.DynamicPrimitiveField(label, effect.Stat);
			string input = SirenixEditorGUI.DynamicPrimitiveField(null, Effect.Convert(effect), GUILayout.MaxWidth(EffectWidth));
			try
			{
				(effect.Type, effect.Value) = Effect.Parse(input);
			}
			catch { }

			ValueEntry.SmartValue = effect;
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}
}