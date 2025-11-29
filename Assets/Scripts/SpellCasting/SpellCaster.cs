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
    public Transform canvasAnchor;

    [Header("Line Settings")]
    public float lineWidth = 0.1f;

    [Header("Time Slowdown Settings")]
    [Range(0.01f, 1f)] public float targetDrawTimeScale = 0.2f;
    public float slowDownSpeed = 5f;

    private float targetTimeScale = 1f;

    private LineRenderer lr;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 lastMousePos;
    private Camera playerCamera;
    private bool strokeActive = false;
    private bool drawingMode = false;

    void Awake()
    {
        playerCamera = Camera.main;
        if (castPoint == null) castPoint = transform;
        if (canvasAnchor == null) canvasAnchor = playerCamera.transform;

        lr = gestureLineObject.GetComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 0;

        drawCanvas.renderMode = RenderMode.WorldSpace;
        drawCanvas.worldCamera = playerCamera;
        drawCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * slowDownSpeed);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (!drawingMode) return;

        UpdateCanvasPosition();

        Vector3 mousePos = Input.mousePosition;
        if (strokeActive && Vector3.Distance(mousePos, lastMousePos) > 8f)
        {
            Vector3 worldPos = ScreenToCanvasLocal(mousePos);
            points.Add(worldPos);
            lr.positionCount = points.Count;
            lr.SetPosition(points.Count - 1, worldPos);
            lastMousePos = mousePos;
        }
    }

    public void StartDrawing()
    {
        drawingMode = true;

        points.Clear();
        lr.positionCount = 0;

        drawCanvas.gameObject.SetActive(true);
        gestureLineObject.SetActive(true);

        lastMousePos = Input.mousePosition;

        targetTimeScale = targetDrawTimeScale;
    }

    public void BeginStroke()
    {
        strokeActive = true;
        lastMousePos = Input.mousePosition;
    }

    public void EndStroke()
    {
        strokeActive = false;
    }

    public void EndDrawing()
    {
        drawingMode = false;

        drawCanvas.gameObject.SetActive(false);
        gestureLineObject.SetActive(false);

        if (points.Count > 10)
            ProcessSpellRecognition();

        points.Clear();

        targetTimeScale = 1f;
    }

    private void ProcessSpellRecognition()
    {
        foreach (var spell in availableSpells)
        {
            float score = Recognize(spell.gestureType);
            if (score >= spell.minScore)
            {
                CastSpell(spell, score);
                return;
            }
        }

        CastSpell(availableSpells[Random.Range(0, availableSpells.Length)], 0f);
    }

    private float Recognize(SpellData.GestureType type)
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

    private void CastSpell(SpellData data, float score)
    {
        bool glitch = score < glitchThreshold;

        var go = Instantiate(
            data.effectPrefab,
            castPoint.position,
            castPoint.rotation
        );

        if (go.TryGetComponent<SpellEffect>(out var fx))
            fx.Initialize(data.baseDamage, glitch, data.speed);
    }

    private Vector3 ScreenToCanvasLocal(Vector3 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawCanvas.GetComponent<RectTransform>(),
            screenPos,
            playerCamera,
            out var lp
        );
        return new Vector3(lp.x, lp.y, 0);
    }

    private void UpdateCanvasPosition()
    {
        drawCanvas.transform.position =
            canvasAnchor.position + canvasAnchor.forward * canvasDistance;

        drawCanvas.transform.rotation = canvasAnchor.rotation;
        drawCanvas.transform.localScale = Vector3.one * 0.001f;
    }
}
