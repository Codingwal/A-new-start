using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSystem : SingletonPersistant<MainSystem>
{
    public GameState gameState;
    public event Action<GameState> GameStateChanged;
    public event Action LoadWorld;
    public event Action Pause;
    public event Action Continue;
    protected override void SingletonAwake()
    {
        switch (SceneManager.GetActiveScene().ToString())
        {
            case "Singleplayer":
                // Making the game think the game was loaded from the MainMenu to prevent unexpected behaviour
                gameState = GameState.MainMenu;
                ChangeGameState(GameState.InGame);
                break;
            case "MainMenu":
                ChangeGameState(GameState.MainMenu);
                break;
        }
    }
    private void OnApplicationQuit()
    {
        ChangeGameState(GameState.Quitting);
    }
    public void ChangeGameState(GameState newGameState)
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
                    DataManager.Instance.LoadWorld();
                    LoadWorld?.Invoke();
                    Debug.Log("Finished loading world");
                }
                break;
            case GameState.MainMenu:
                Debug.Log("Loading MainMenu");
                DataManager.Instance.SaveWorld();
                break;
            case GameState.Paused:
                Pause?.Invoke();
                break;
            case GameState.Quitting:
                Debug.Log("Quitting");
                DataManager.Instance.SaveWorld();
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
