using UnityEngine;

public abstract class SpellEffect : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public bool isGlitch;

    public virtual void Initialize(float dmg, bool glitch, float speed = 10f)
    {
        damage = dmg;
        isGlitch = glitch;
        if (isGlitch) transform.localScale *= Random.Range(0.8f, 1.5f);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        DummyEnemy enemy = other.GetComponent<DummyEnemy>();
        if (enemy != null)
        {
            float finalDamage = damage;
            if (isGlitch) finalDamage *= Random.Range(0.5f, 2f);
            enemy.TakeDamage(finalDamage, isGlitch);
        }
        Destroy(gameObject);
    }
}