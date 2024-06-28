using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    public float jumpForce = 4f;
    [Space]
    public float walkSpeed = 5f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 3f;
    private float speed;

    private bool isGrounded;
    private float floorAngle;

    private void Start()
    {
        InputManager.Instance.Move += ProcessMove;
        InputManager.Instance.Jump += Jump;
    }
    private void OnCollisionStay(Collision collision)
    {
        float minDistance = float.PositiveInfinity;
        floorAngle = 180;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            float distance = Vector3.Distance(contact.point, transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;

                floorAngle = Vector3.Angle(transform.up, contact.normal);
            }
        }
        isGrounded = floorAngle <= 45;
    }
    private void OnCollisionExit(Collision other)
    {
        floorAngle = 180;
        isGrounded = false;
    }
    public void ProcessMove(Vector2 input)
    {
        // Set speed depending on the walkState
        switch (InputManager.Instance.walkState)
        {
            case WalkState.walking:
                speed = walkSpeed;
                break;
            case WalkState.sprinting:
                speed = sprintSpeed;
                break;
            case WalkState.crouching:
                speed = crouchSpeed;
                break;
        }
        Vector3 velocity = input.x * transform.right + input.y * transform.forward;

        rb.MovePosition(transform.position + (speed * Time.deltaTime * velocity));
    }
    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }
}
