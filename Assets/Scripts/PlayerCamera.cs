using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private Transform cameraCentreTransform = null;
    [SerializeField] private Transform cameraTransform = null;

    [Header("Config")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1, 0);
    [SerializeField] private float followLerpSpeed = 8;
    [SerializeField] private float zoomLerpSpeed = 16;
    [SerializeField] private float zoomStrength = 0.06f;
    [SerializeField] private float zoomMin = 0.5f;
    [SerializeField] private float zoomMax = 2;
    [SerializeField] private float swayEaseScale = 0.5f;
    [SerializeField] private float swayAmount = 5;
    [SerializeField] private float swayLerp = 10;
    [SerializeField] private float swayDeadzone = 0.05f;

    private Quaternion initialCameraRot;
    private Vector3 initialOffset;
    private float zoomAmount = 1.0f;

    private void Start()
    {
        // Set camera position to player position
        cameraCentreTransform.position = playerController.transform.position + followOffset;
        initialCameraRot = cameraTransform.rotation;
        initialOffset = cameraTransform.localPosition;
    }

    private void Update()
    {
        // Handle mouse scroll for zooming
        float scroll = Input.mouseScrollDelta.y;
        zoomAmount = zoomAmount * (1.0f - scroll * zoomStrength);
        zoomAmount = Mathf.Clamp(zoomAmount, zoomMin, zoomMax);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialOffset * zoomAmount, zoomLerpSpeed * Time.deltaTime);

        // Calculate current mouse offset from centre
        Vector2 mouseOffset = Vector2.zero;
        mouseOffset.x = Mathf.Clamp01(Input.mousePosition.x / Screen.width) - 0.5f;
        mouseOffset.y = Mathf.Clamp01(Input.mousePosition.y / Screen.height) - 0.5f;

        // Calculate deadzoned and scaled sway for mouse
        float xRot = 0;
        float yRot = 0;
        if (Mathf.Abs(mouseOffset.y) > swayDeadzone)
            xRot = -Easing.EaseOutQuad(swayEaseScale * (Mathf.Abs(mouseOffset.y) - swayDeadzone)) * Mathf.Sign(mouseOffset.y) * swayAmount;
        if (Mathf.Abs(mouseOffset.x) > swayDeadzone)
            yRot = Easing.EaseOutQuad(swayEaseScale * (Mathf.Abs(mouseOffset.x) - swayDeadzone)) * Mathf.Sign(mouseOffset.x) * swayAmount;

        // Lerp camera rotation towards sway rotation
        if (xRot != 0 || yRot != 0)
        {
            Quaternion swayRotation = Quaternion.Euler(xRot, yRot, 0);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, initialCameraRot * swayRotation, swayLerp * Time.deltaTime);
        }

        // Lerp back to initial camera rotation
        else cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, initialCameraRot, swayLerp * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Lerp camera position towards player position
        cameraCentreTransform.position = Vector3.Lerp(cameraCentreTransform.position, playerController.transform.position + followOffset, followLerpSpeed * Time.fixedDeltaTime);
    }
}
