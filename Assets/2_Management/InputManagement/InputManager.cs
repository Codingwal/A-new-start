using System;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput playerInput;
    private PlayerInput.GameplayActions gameplay;

    public WalkState walkState;
    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    public event Action Jump;
    
    protected override void SingletonAwake()
    {
        playerInput = new();
        gameplay = playerInput.Gameplay;

        MainSystem.GameStateChanged += OnGameStateChanged;
    }
    private void OnDisable() {
        MainSystem.GameStateChanged -= OnGameStateChanged;
    }   
    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState == GameState.InGame)
        {
            Debug.Log("Enabling Gameplay");
            gameplay.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Debug.Log("Disabling Gameplay");
            gameplay.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    private void Update()
    {
        if (gameplay.Pause.IsPressed())
        {
            MainSystem.ChangeGameState(GameState.Paused);
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
