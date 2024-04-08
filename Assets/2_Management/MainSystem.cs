using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainSystem
{
    public static GameState gameState;
    public static event Action<GameState> GameStateChanged;
    public static event Action LoadWorld;
    public static event Action Pause;
    public static event Action Continue;
    
    public static void ChangeGameState(GameState newGameState)
    {
        switch (newGameState)
        {
            case GameState.InGame:
                if (gameState == GameState.Paused)
                {
                    Debug.Log("Continue");
                    Continue?.Invoke();
                }
                else if (gameState == GameState.MainMenu)
                {
                    Debug.Log("Starting game from MainMenu");
                    DataManager.LoadWorld();
                    LoadWorld?.Invoke();
                    Debug.Log("Finished loading world");
                }
                break;
            case GameState.MainMenu:
                Debug.Log("Loading MainMenu");
                DataManager.SaveWorld();
                break;
            case GameState.Paused:
                Pause?.Invoke();
                break;
            case GameState.Quitting:
                Debug.Log("Quitting");
                DataManager.SaveWorld();
                break;
        }

        gameState = newGameState;
        GameStateChanged?.Invoke(newGameState);
    }
}
public enum GameState
{
    MainMenu,
    Paused,
    InGame,
    Quitting
}
