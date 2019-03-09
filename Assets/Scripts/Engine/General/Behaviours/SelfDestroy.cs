using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
	public float Life = 5.0f;

	protected void Start()
	{
		Destroy(gameObject, Life);
	}
}