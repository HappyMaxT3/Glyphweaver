using UnityEngine;

public class Fireball : SpellEffect
{
    [Header("Fireball Properties")]
    public float lifeTime = 5f;
    public float damage = 10f;

    [Header("Effects")]
    public ParticleSystem trailEffect;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public override void Initialize(float baseDamage, bool isGlitch, float speedFromCaster)
    {
        this.damage = baseDamage;

        rb.linearVelocity = transform.forward * speedFromCaster;

        if (trailEffect != null)
        {
            var main = trailEffect.main;

            main.startColor = isGlitch ? Color.red : new Color(1f, 0.5f, 0f);
            if (!trailEffect.isPlaying) trailEffect.Play();
        }

        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {

        Destroy(gameObject);
    }
}