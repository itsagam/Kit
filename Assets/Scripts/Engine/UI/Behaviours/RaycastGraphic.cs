using UnityEditor;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
#if UNITY_EDITOR
	[CustomEditor(typeof(RaycastGraphic), false)]
	public class RaycastGraphicEditor : Editor
	{
		public override void OnInspectorGUI() { }
	}
#endif

	/// A concrete subclass of the Unity UI `Graphic` class that just skips drawing.
	/// Useful for providing a raycast target without actually drawing anything.
	public class RaycastGraphic : Graphic
	{
		public override void SetMaterialDirty() {}
		public override void SetVerticesDirty() {}

		/// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}