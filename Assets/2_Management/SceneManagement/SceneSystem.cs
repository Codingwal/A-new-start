using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSystem : Singleton<SceneSystem>
{
    public event Action<Scenes> SceneSwitched;

    public event Action<float> Loading;
    public float progress;
    public void SwitchScene(Scenes scene)
    {
        StartCoroutine(Instance.LoadSceneAsynchronously(scene));
    }
    private IEnumerator LoadSceneAsynchronously(Scenes scene)
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
                MainSystem.Instance.ChangeGameState(GameState.MainMenu);
                break;
            case Scenes.Singleplayer:
                MainSystem.Instance.ChangeGameState(GameState.InGame);
                break;
        }
    }
    public enum Scenes
    {
        MainMenu,
        Singleplayer
    }

}
