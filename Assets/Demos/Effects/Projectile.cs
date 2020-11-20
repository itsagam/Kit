using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 20.0f;
    private new Transform transform;

    void Awake()
    {
        transform = base.transform;
    }

    void Update()
    {
        transform.Translate(transform.up * Speed * Time.deltaTime);
    }
}
