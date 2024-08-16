using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 InputDir { get; private set; } = Vector3.zero;
    public bool IsMoving => InputDir != Vector3.zero;
    public bool IsSprinting => Input.GetKey(KeyCode.LeftShift);

    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera = null;

    [Header("Config")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.2f;
    [SerializeField] private float rotationSpeed = 200f;

    private void Update()
    {
        // Receive input from the player
        Vector3 inputDir = Vector3.zero;

        // Cast camera transform onto flat plane
        Vector3 forwardDir = Vector3.ProjectOnPlane(playerCamera.CameraTransform.forward, Vector3.up).normalized;
        inputDir += Input.GetAxisRaw("Horizontal") * playerCamera.CameraTransform.right;
        inputDir += Input.GetAxisRaw("Vertical") * forwardDir;
        InputDir = inputDir.normalized;
    }

    private void FixedUpdate()
    {
        // Squish character to show sprinting
        float squishAmount = IsSprinting ? 0.9f : 1.0f;
        transform.localScale = new Vector3(1, squishAmount, 1);

        if (!IsMoving) return;

        // Rotate towards input direction
        Quaternion targetRotation = Quaternion.LookRotation(InputDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Move in input direction
        float speed = IsSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
        transform.position += InputDir * speed * Time.fixedDeltaTime;
    }
}
