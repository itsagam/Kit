using UnityEngine;

namespace Engine.Behaviours
{
	public class QuitOnBack: MonoBehaviour
	{
#if UNITY_ANDROID
	protected void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
#endif
	}
}