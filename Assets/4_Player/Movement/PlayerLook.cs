using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    private InputManager inputManager;
    public Camera cam;
    [HideInInspector] public float xRotation = 0f;

    public float xSensitivity;
    public float ySensitivity;

    private void Start()
    {
        inputManager = InputManager.Instance;
        inputManager.Look += ProcessLook;
    }
    public void ProcessLook(Vector2 input)
    {
        // Apply up/down rotation only to the camera, rotation around the x axis to the player which will also rotate the camera
        xRotation -= input.y * Time.deltaTime * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(input.x * Time.deltaTime * xSensitivity * Vector3.up);
    }
}
