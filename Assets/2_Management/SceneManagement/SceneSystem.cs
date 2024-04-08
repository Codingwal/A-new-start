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
        justToExecCoroutine.StartCoroutine(LoadSceneAsynchronously(scene));
    }
    private static IEnumerator LoadSceneAsynchronously(Scenes scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene.ToString());

        while (!operation.isDone)
        {
            progress = operation.progress / 0.9f;

            Loading?.Invoke(progress);
            yield return null;
        }
        Debug.Log(scene.ToString() + " has finished loading");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.ToString()));
        SceneSwitched?.Invoke(scene);

        switch (scene)
        {
            case Scenes.MainMenu:
                MainSystem.ChangeGameState(GameState.MainMenu);
                break;
            case Scenes.Singleplayer:
                MainSystem.ChangeGameState(GameState.InGame);
                break;
        }
    }
    public enum Scenes
    {
        MainMenu,
        Singleplayer
    }

}
