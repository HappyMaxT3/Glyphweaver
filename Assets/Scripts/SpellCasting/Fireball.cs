using UnityEngine;

public class Fireball : SpellEffect
{
    public ParticleSystem trailEffect;

    public override void Initialize(float dmg, bool glitch, float speed)
    {
        base.Initialize(dmg, glitch);
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;

        if (trailEffect != null)
        {
            var main = trailEffect.main;
            main.startColor = glitch ? Color.red : Color.yellow;
        }
    }
}