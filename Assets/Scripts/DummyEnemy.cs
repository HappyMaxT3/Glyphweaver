using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage, bool isGlitch = false)
    {
        health -= damage;
        Debug.Log($"Dummy hit! Damage: {damage}, Glitch: {isGlitch}, Health left: {health}");
        if (health <= 0) Destroy(gameObject);
    }
}