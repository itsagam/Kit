using UnityEditor;
using UnityEngine.UI;

namespace Kit.UI.Behaviours
{
#if UNITY_EDITOR
	[CustomEditor(typeof(RaycastGraphic), false)]
	public class RaycastGraphicEditor: Editor
	{
		public override void OnInspectorGUI() { }
	}
#endif

	/// <summary>
	///     A sub-class of the Unity UI <see cref="Graphic" /> that just skips drawing. Useful for providing a raycast target without actually
	///     drawing anything.
	/// </summary>
	public class RaycastGraphic: Graphic
	{
		public override void SetMaterialDirty() { }
		public override void SetVerticesDirty() { }

		/// Probably not necessary since the chain of calls Rebuild()->UpdateGeometry()->DoMeshGeneration()->OnPopulateMesh() won't happen,
		/// but here really just as a fail-safe.
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();
		}
	}
}