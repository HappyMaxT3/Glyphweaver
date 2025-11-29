using PlayerInputActions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    [Header("Drawing State")]
    public bool isDrawing = false;

    private Rigidbody rb;
    private Camera playerCamera;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private PlayerControllers inputActions;
    private SpellCaster spellCaster;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        spellCaster = GetComponent<SpellCaster>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        inputActions = new PlayerControllers();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.DrawMode.performed += OnDrawModePerformed;
        inputActions.Player.DrawMode.canceled += OnDrawModeCanceled;

        inputActions.Player.DrawStroke.performed += OnDrawStrokePerformed;
        inputActions.Player.DrawStroke.canceled += OnDrawStrokeCanceled;
    }

    void Update()
    {
        if (isDrawing) return;

        transform.Rotate(0, lookInput.x * lookSensitivity * Time.deltaTime, 0);
        playerCamera.transform.Rotate(-lookInput.y * lookSensitivity * Time.deltaTime, 0, 0);

        Vector3 camRot = playerCamera.transform.localEulerAngles;
        camRot.x = Mathf.Clamp(camRot.x, -90f, 90f);
        if (camRot.x > 180f) camRot.x -= 360f;
        playerCamera.transform.localEulerAngles = camRot;
    }

    void FixedUpdate()
    {
        if (isDrawing) return;

        Vector3 moveDir = transform.forward * moveInput.y + transform.right * moveInput.x;
        rb.MovePosition(rb.position + moveDir.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnMove(InputAction.CallbackContext ctx) =>
        moveInput = ctx.ReadValue<Vector2>();

    private void OnLook(InputAction.CallbackContext ctx) =>
        lookInput = ctx.ReadValue<Vector2>();

    private void OnDrawModePerformed(InputAction.CallbackContext ctx)
    {
        spellCaster.StartDrawing();
        isDrawing = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDrawModeCanceled(InputAction.CallbackContext ctx)
    {
        spellCaster.EndDrawing();
        isDrawing = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDrawStrokePerformed(InputAction.CallbackContext ctx)
    {
        if (isDrawing) spellCaster.BeginStroke();
    }

    private void OnDrawStrokeCanceled(InputAction.CallbackContext ctx)
    {
        if (isDrawing) spellCaster.EndStroke();
    }
}
