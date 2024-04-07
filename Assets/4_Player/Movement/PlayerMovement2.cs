using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    public WalkState2 walkState = WalkState2.walking;
    private Vector3 playerVelocity;
    public bool isGrounded;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    public float walkSpeed = 5f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 3f;
    private float speed;

    public bool IsGrounded
    {
        get
        {
            return Physics.Raycast(transform.position, Vector3.down, 1.5f, 0);
        }
    }

    public void ProcessMove(Vector2 input)
    {

        switch (walkState)
        {
            case WalkState2.walking:
                speed = walkSpeed;
                break;
            case WalkState2.sprinting:
                speed = sprintSpeed;
                break;
            case WalkState2.crouching:
                speed = crouchSpeed;
                break;
        }
        
        // Calculate the velocity 
        playerVelocity = transform.TransformDirection(input) * speed * Time.deltaTime;

        // Apply gravity
        playerVelocity.y = gravity * Time.deltaTime;

        // Reduce gravity if the player is already on the ground
        if (IsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        
    }
    public void Jump()
    {
        if (IsGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3 * gravity);
        }
    }
}

public enum WalkState2
{
    walking,
    sprinting,
    crouching
}
