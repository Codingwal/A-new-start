using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSystem
{
    class CoroutineDummy : MonoBehaviour { }
    public static event Action<Scenes> SceneSwitched;

    public static event Action<float, string> Loading;
    public static float progress;

    private static Action onLoaderCallback;
    public static int chunksWaitingForMapDataCount;
    public static void LoadSingleplayer()
    {
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new("Loading Game Object");
            loadingGameObject.AddComponent<CoroutineDummy>().StartCoroutine(LoadSingleplayerAsync());

            Debug.Log("Singleplayer has finished loading");
            SceneSwitched?.Invoke(Scenes.Singleplayer);
        };
        SceneManager.LoadScene(Scenes.Loading.ToString());
    }
    private static IEnumerator LoadSingleplayerAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(Scenes.Singleplayer.ToString(), LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            progress = operation.progress / 0.9f;
            Loading?.Invoke(progress, "Loading Scene");
            yield return null;
        }
        while (chunksWaitingForMapDataCount != 0)
        {
            progress = (float)(49 - chunksWaitingForMapDataCount) / 49;
            Loading?.Invoke(progress, "Generating Terrain");
            yield return null;
        }
        Loading?.Invoke(1, "Starting game");
        yield return null;
        SceneManager.UnloadSceneAsync(Scenes.Loading.ToString());
    }
    public static void LoaderCallback()
    {
        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }

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

            Loading?.Invoke(progress, "Saving Game");
            yield return null;
        }
    }
    public enum Scenes
    {
        MainMenu,
        Singleplayer,
        Loading
    }

}
