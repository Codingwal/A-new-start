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
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= mouseY * Time.deltaTime * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseX * Time.deltaTime * xSensitivity);
    }
}
