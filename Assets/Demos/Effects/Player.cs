using System;
using Kit;
using Kit.Behaviours;
using Kit.Pooling;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PoolGroup Group;
    public Pool Pool;
    public MoveInDirection Projectile;
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
        {
            //transform.up = -direction;
            transform.Translate(direction * (Speed * Time.deltaTime));
        }

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        Pool pool = null;

        if (Group != null)
            pool = Group.Pools.GetRandom();

        if (pool == null)
            pool = Pool;

        if (pool != null)
            pool.Instantiate(transform.position);
        else
            Pooler.Instantiate(Projectile, transform.position);
    }
}
