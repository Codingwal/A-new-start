using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuMaster : MonoBehaviour
{
    public GameObject player;
    public GameObject pauseMenu;
    [Space]
    public GameObject loadingScreen;
    public Slider progressSlider;
    public TMP_Text progressText;
    private void Awake()
    {
        SceneSystem.Loading += OnSceneLoading;
    }
    private void Start()
    {
        Continue();

        MainSystem.Pause += Pause;
    }
    private void OnDisable()
    {
        SceneSystem.Loading -= OnSceneLoading;
        MainSystem.Pause -= Pause;
    }
    public void Pause()
    {
        pauseMenu.SetActive(true);
    }
    public void Continue()
    {
        pauseMenu.SetActive(false);
        MainSystem.ChangeGameState(GameState.InGame);
    }
    public void OpenMainMenu()
    {
        loadingScreen.SetActive(true);
        SceneSystem.SwitchScene(SceneSystem.Scenes.MainMenu, this);
    }
    public void OnSceneLoading(float progress, string currentTask)
    {
        progressSlider.value = progress;
        progressText.text = progress * 100 + "%";
    }
}
