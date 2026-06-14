using UnityEngine;

public class ExhibitionPlayer : MonoBehaviour
{
    public float walkSpeed = 6.0f;
    public float gravity = 20.0f;
    public float lookSensitivity = 2.0f;
    public Transform cam;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Mouse ko screen ke beech mein lock karne ke liye
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. LOOK AROUND (Mouse)
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        cam.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * mouseX);

        // 2. MOVEMENT (WASD)
        if (controller.isGrounded)
        {
            // Direct input lene se sliding stop ho jati hai
            float moveX = Input.GetAxisRaw("Horizontal"); // GetAxisRaw use karein instant stop ke liye
            float moveZ = Input.GetAxisRaw("Vertical");

            moveDirection = (transform.forward * moveZ) + (transform.right * moveX);
            moveDirection *= walkSpeed;
        }

        // Apply Gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Final Move call
        controller.Move(moveDirection * Time.deltaTime);
    }
}