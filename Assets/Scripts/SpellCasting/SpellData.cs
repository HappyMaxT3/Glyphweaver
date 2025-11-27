using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellData", menuName = "SpellData")]
public class SpellData : ScriptableObject
{
    public string spellName = "SpellName";
    public float minScore = 0.7f;
    public GameObject effectPrefab;
    public float baseDamage = 50f;
    public float speed = 10f;
    [Header("Gesture Type")]
    public GestureType gestureType = GestureType.Circle;

    public enum GestureType { Circle, Line, Zigzag }
}