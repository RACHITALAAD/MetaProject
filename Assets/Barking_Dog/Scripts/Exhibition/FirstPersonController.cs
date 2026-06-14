using UnityEngine;

/// <summary>
/// ATTACH TO: Player GameObject
/// REQUIRES:  CharacterController component on Player
///
/// SETUP IN INSPECTOR:
///   cameraTransform → drag Camera (child of Player)
///   groundCheck     → drag GroundCheck (child of Player)
///   groundMask      → tick Default layer
///   walkSpeed       → 4
///   mouseSensitivity→ 2
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Drag your Camera child here")]
    public Transform cameraTransform;

    [Header("Drag GroundCheck child here")]
    public Transform groundCheck;

    [Header("Settings")]
    public float walkSpeed        = 4f;
    public float mouseSensitivity = 2f;
    public float groundDistance   = 0.3f;
    public LayerMask groundMask;

    private CharacterController cc;
    private float   xRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        // Start locked (FPS mode)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        // ── Gravity ───────────────────────────────────────────────
        bool grounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)
            : cc.isGrounded;

        if (grounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += -9.81f * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        // ── STOP here if cursor is NOT locked ─────────────────────
        // This covers: catalog open, panels open, artwork selected
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // ── Mouse Look (360 degrees) ──────────────────────────────
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the Player body left and right
        transform.Rotate(Vector3.up, mouseX);

        // Rotate Camera up and down (clamped to avoid flipping)
        xRotation -= mouseY;
        xRotation  = Mathf.Clamp(xRotation, -85f, 85f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ── WASD Movement ─────────────────────────────────────────
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        Vector3 move = transform.right   * h
                     + transform.forward * v;
        cc.Move(move * walkSpeed * Time.deltaTime);
    }
}
