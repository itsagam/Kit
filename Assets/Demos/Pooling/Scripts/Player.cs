using Kit;
using Kit.Pooling;
using UnityEngine;

namespace Demos.Pooling
{
    public class Player : Ship
    {
        public Projectile Projectile;
        public float Speed = 10.0f;
        public AudioClip Fire1Sound;
        public AudioClip Fire2Sound;

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
            Projectile projectile = Pooler.Instantiate(Projectile, transform.position);
            AudioManager.PlaySound(Fire1Sound);
        }

        void Fire2()
        {
            Projectile projectile = Pooler.Instantiate(Projectile, transform.position);
            projectile.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 180) - 90);
            AudioManager.PlaySound(Fire2Sound);
        }
    }
}