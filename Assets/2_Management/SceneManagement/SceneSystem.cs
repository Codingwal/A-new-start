using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSystem
{
    public static event Action<Scenes> SceneSwitched;

    public static event Action<float> Loading;
    public static float progress;
    public static void SwitchScene(Scenes scene, MonoBehaviour justToExecCoroutine)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene.ToString());

        justToExecCoroutine.StartCoroutine(LoadSceneAsynchronously(operation));

        Debug.Log(scene.ToString() + " has finished loading");
        SceneSwitched?.Invoke(scene);

        switch (scene)
        {
            case Scenes.MainMenu:
                MainSystem.ChangeGameState(GameState.MainMenu);
                break;
            case Scenes.Singleplayer:
                // MainSystem.ChangeGameState(GameState.InGame);
                break;
            default:
                throw new();
        }
    }
    private static IEnumerator LoadSceneAsynchronously(AsyncOperation operation)
    {
        while (!operation.isDone)
        {
            progress = operation.progress / 0.9f;

            Loading?.Invoke(progress);
            yield return null;
        }
    }
    public enum Scenes
    {
        MainMenu,
        Singleplayer
    }

}
