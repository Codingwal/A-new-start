using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput playerInput;
    private PlayerInput.GameplayActions gameplay;

    public WalkState walkState;
    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    public event Action Jump;
    public static event Action ToggleDevSprint;
    public static event Action DevJump;
    public static event Action ToggleDebug;

    protected override void SingletonAwake()
    {
        playerInput = new();
        gameplay = playerInput.Gameplay;

        MainSystem.GameStateChanged += OnGameStateChanged;

        gameplay.Jump.started += OnJumpStarted;
        gameplay.Debug.started += OnDebugStarted;
        gameplay.Sprint.started += OnSprintStarted;
    }
    private void OnDisable()
    {
        MainSystem.GameStateChanged -= OnGameStateChanged;

        gameplay.Jump.started -= OnJumpStarted;
        gameplay.Debug.started -= OnDebugStarted;
        gameplay.Sprint.started -= OnSprintStarted;
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
    }
    private void LateUpdate()
    {
        Look?.Invoke(gameplay.Look.ReadValue<Vector2>());
    }
    void OnJumpStarted(CallbackContext ctx)
    {
        if (gameplay.Debug.IsPressed())
        {
            DevJump?.Invoke();
        }
        else
        {
            Jump?.Invoke();
        }
    }
    void OnDebugStarted(CallbackContext ctx)
    {
        ToggleDebug?.Invoke();
    }
    void OnSprintStarted(CallbackContext ctx)
    {
        if (gameplay.Debug.IsPressed())
        {
            ToggleDevSprint?.Invoke();
        }
    }
}
public enum WalkState
{
    walking,
    sprinting,
    crouching
}
