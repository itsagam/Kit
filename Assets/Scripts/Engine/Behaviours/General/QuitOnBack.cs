using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Quits the application when the Back button is pressed on Android.
	/// </summary>
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