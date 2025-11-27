using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SpellCaster : MonoBehaviour
{
    [Header("Spells")]
    public SpellData[] availableSpells = new SpellData[1];
    public float glitchThreshold = 0.5f;
    public Transform castPoint;

    [Header("Drawing Canvas")]
    public Canvas drawCanvas;
    public GameObject gestureLineObject;
    public float canvasDistance = 2f;

    [Header("Line Settings")]
    public float lineWidth = 0.1f;

    private LineRenderer lr;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 lastMousePos;
    private Camera playerCamera;

    void Awake()
    {
        playerCamera = Camera.main;
        if (castPoint == null) castPoint = transform;
        if (drawCanvas == null || gestureLineObject == null)
        {
            Debug.LogError("Canvas or GestureLine not assigned!");
            enabled = false;
            return;
        }

        lr = gestureLineObject.GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.LogError("LineRenderer component missing!");
            enabled = false;
            return;
        }

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 0;

        drawCanvas.renderMode = RenderMode.WorldSpace;
        drawCanvas.worldCamera = playerCamera;
        drawCanvas.gameObject.SetActive(false);
    }

    public void StartDrawing()
    {
        points.Clear();
        lr.positionCount = 0;

        UpdateCanvasPosition();
        drawCanvas.gameObject.SetActive(true);
        gestureLineObject.SetActive(true);

        lastMousePos = Input.mousePosition;
    }

    void Update()
    {
        if (!drawCanvas.gameObject.activeSelf) return;

        UpdateCanvasPosition();

        Vector3 mousePos = Input.mousePosition;
        if (Vector3.Distance(mousePos, lastMousePos) > 8f)
        {
            Vector3 worldPos = ScreenToCanvasLocal(mousePos);
            points.Add(worldPos);
            lr.positionCount = points.Count;
            lr.SetPosition(points.Count - 1, worldPos);
            lastMousePos = mousePos;
        }
    }

    Vector3 ScreenToCanvasLocal(Vector3 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawCanvas.GetComponent<RectTransform>(),
            screenPos,
            playerCamera,
            out localPoint
        );
        return new Vector3(localPoint.x, localPoint.y, 0);
    }

    void UpdateCanvasPosition()
    {
        Vector3 canvasPos = playerCamera.transform.position + playerCamera.transform.forward * canvasDistance;
        drawCanvas.transform.position = canvasPos;
        drawCanvas.transform.rotation = playerCamera.transform.rotation;
        drawCanvas.transform.localScale = Vector3.one * 0.001f;
    }

    public void EndDrawing()
    {
        drawCanvas.gameObject.SetActive(false);
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
        foreach (var p in points) p2d.Add(new Vector2(p.x, p.y));
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