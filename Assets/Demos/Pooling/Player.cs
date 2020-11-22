using Demos.Pooling;
using Kit;
using Kit.Pooling;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Projectile Prefab;
    public float Speed = 10.0f;
    private new Transform transform;

    void Awake()
    {
        transform = base.transform;
    }

    void Update()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");
        Vector2 direction = new Vector2(xAxis, yAxis);

        if (direction.sqrMagnitude > 0)
            transform.Translate(direction * (Speed * Time.deltaTime));

        if (Input.GetButtonDown("Fire1"))
            Fire1();

        if (Input.GetButtonDown("Fire2"))
            Fire2();
    }

    void Fire1()
    {
        Pooler.GetGroup("Projectiles").Pools.GetRandom().Instantiate(transform.position);
    }

    void Fire2()
    {
        Projectile instance = Pooler.Instantiate(Prefab, Vector3.zero);
        instance.transform.up = Random.insideUnitCircle.normalized;
    }
}
