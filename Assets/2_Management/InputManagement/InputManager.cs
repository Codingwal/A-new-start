using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput playerInput;
    private PlayerInput.GameplayActions gameplay;

    public WalkState walkState;
    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    public event Action Jump;
    
    protected void Start()
    {
        playerInput = new();
        gameplay = playerInput.Gameplay;

        MainSystem.Instance.GameStateChanged += OnGameStateChanged;
    }
    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState == GameState.InGame)
        {
            gameplay.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            gameplay.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    private void Update()
    {
        if (gameplay.Pause.IsPressed())
        {
            MainSystem.Instance.ChangeGameState(GameState.Paused);
        }
        if (gameplay.Sprint.IsPressed())
        {
            walkState = WalkState.sprinting;
        }
        else if (gameplay.Crouch.IsPressed())
        {
            walkState = WalkState.crouching;
        }
        else
        {
            walkState = WalkState.walking;
        }
    }
    private void FixedUpdate()
    {
        Move?.Invoke(gameplay.Move.ReadValue<Vector2>());

        if (gameplay.Jump.IsPressed())
        {
            Jump?.Invoke();
        }
    }
    private void LateUpdate()
    {
        Look?.Invoke(gameplay.Look.ReadValue<Vector2>());
    }
}
public enum WalkState
{
    walking,
    sprinting,
    crouching
}
