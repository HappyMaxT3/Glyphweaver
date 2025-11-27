using UnityEngine;

public abstract class SpellEffect : MonoBehaviour
{
    public abstract void Initialize(float baseDamage, bool isGlitch, float speed);
}