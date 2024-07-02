using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 InputDir { get; private set; } = Vector3.zero;
    public bool IsMoving => InputDir != Vector3.zero;
    public bool IsSprinting => Input.GetKey(KeyCode.LeftShift);

    [Header("Config")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.2f;
    [SerializeField] private float rotationSpeed = 200f;

    private void Update()
    {
        // Receive input from the player
        Vector3 inputDir = Vector3.zero;
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.z = Input.GetAxisRaw("Vertical");
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
