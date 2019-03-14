using UnityEngine;

namespace Engine.Behaviours
{
	public class FadeIn : MonoBehaviour
	{
		protected void Awake()
		{
			SceneDirector.FadeIn();
		}
	}
}