using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SpellCaster : MonoBehaviour
{
    public SpellData[] availableSpells = new SpellData[1];
    public float glitchThreshold = 0.5f;
    public Transform castPoint;
    public GameObject gestureLineObject;
    public Color lineColor = Color.cyan;
    public float lineWidth = 20f;

    private LineRenderer lr;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 lastMousePos;

    void Awake()
    {
        if (castPoint == null) castPoint = transform;

        if (gestureLineObject == null)
        {
            Debug.LogError("SpellCaster: GestureLine object not assigned");
            enabled = false;
            return;
        }

        lr = gestureLineObject.GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.LogError("LineRenderer component missing on GestureLine");
            enabled = false;
            return;
        }

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.positionCount = 0;

        gestureLineObject.SetActive(false);
    }

    public void StartDrawing()
    {
        points.Clear();
        lr.positionCount = 0;
        gestureLineObject.SetActive(true);
        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
        if (!gestureLineObject.activeSelf) return;

        Vector3 mousePos = Input.mousePosition;

        if (Vector3.Distance(mousePos, lastMousePos) > 8f)
        {
            points.Add(mousePos);
            lr.positionCount = points.Count;
            lr.SetPosition(points.Count - 1, mousePos);
            lastMousePos = mousePos;
        }
    }

    public void EndDrawing()
    {
        gestureLineObject.SetActive(false);

        if (points.Count > 10)
        {
            foreach (var spell in availableSpells)
            {
                float score = Recognize(spell.gestureType);
                if (score >= spell.minScore)
                {
                    CastSpell(spell, score);
                    points.Clear();
                    return;
                }
            }
            if (availableSpells.Length > 0)
                CastSpell(availableSpells[Random.Range(0, availableSpells.Length)], 0f);
        }

        points.Clear();
    }

    float Recognize(SpellData.GestureType type)
    {
        List<Vector2> p2d = new List<Vector2>();
        foreach (var p in points) p2d.Add(p);

        return type switch
        {
            SpellData.GestureType.Circle => GestureRecognizer.IsCircle2D(p2d).score,
            SpellData.GestureType.Line => GestureRecognizer.IsLine2D(p2d).score,
            _ => 0f
        };
    }

    void CastSpell(SpellData data, float score)
    {
        bool glitch = score < glitchThreshold;
        var go = Instantiate(data.effectPrefab, castPoint.position, castPoint.rotation);
        if (go.TryGetComponent<SpellEffect>(out var fx))
            fx.Initialize(data.baseDamage, glitch, data.speed);

        Debug.Log($"Casting: {data.spellName} | Accuracy: {score:F2}");
    }
}