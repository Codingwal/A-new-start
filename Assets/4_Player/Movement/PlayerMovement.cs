using UnityEngine;

// Inspired by https://www.youtube.com/watch?v=f473C43s8nE (FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial) by Dave/GameDevelopment
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [Header("Movement settings")]
    [SerializeField] float walkSpeed;
    public float sprintSpeed;       // Can be modified by DebugScreen.cs
    [SerializeField] float crouchSpeed;
    float speed;

    [Header("Jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float maxJumpSlope;
    bool canJump;
    float floorAngle;

    private void Start()
    {
        InputManager.Instance.Move += ProcessMove;
        InputManager.Instance.Jump += Jump;
    }
    private void OnCollisionStay(Collision collision)
    {
        // Calculate the ground angle

        float minDistance = float.PositiveInfinity;
        floorAngle = 180;

        // Find the closest point to the players feet and its angle
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            // Calculate the distance to the players feet
            float distance = Vector3.Distance(contact.point, transform.position - new Vector3(0, 1, 0));

            // If this is the new closest collisionPoint, save its angle
            if (distance < minDistance)
            {
                minDistance = distance;
                floorAngle = Vector3.Angle(transform.up, contact.normal);
            }
        }

        // If the angle (slope) exceeds maxJumpSlope (is too steep), set canJump to false
        canJump = floorAngle <= maxJumpSlope;
    }
    private void OnCollisionExit(Collision other)
    {
        // If the player no longer has contact to another object, disable jumping
        floorAngle = 180;
        canJump = false;
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

        // Calculate the 2d movement by using the user input
        Vector3 movement = input.x * transform.right + input.y * transform.forward;

        // rb.velocity = speed * movement.normalized + new Vector3(0, rb.velocity.y, 0);
        rb.AddForce(speed * movement.normalized * 10f, ForceMode.Force);

        Vector2 floatVelocity = new(rb.velocity.x, rb.velocity.z);
        if (floatVelocity.magnitude > speed)
        {
            Vector2 limitedVelocity = floatVelocity.normalized * speed;
            rb.velocity = new(limitedVelocity.x, rb.velocity.y, limitedVelocity.y);
        }
    }
    public void Jump()
    {
        if (canJump)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }
}
