using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RaycastGraphic), false)]
public class RaycastGraphicEditor : Editor
{
	public override void OnInspectorGUI() { }
}
#endif

/// A concrete subclass of the Unity UI `Graphic` class that just skips drawing.
/// Useful for providing a raycast target without actually drawing anything.
[HideInInspector]
public class RaycastGraphic : Graphic
{
	public override void SetMaterialDirty() { return; }
	public override void SetVerticesDirty() { return; }

	/// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		return;
	}
}