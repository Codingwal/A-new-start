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
    private void Awake() {
        SceneSystem.Instance.Loading += OnSceneLoading;
    }
    private void Start()
    {
        Continue();

        MainSystem.Instance.Pause += Pause;
    }
    public void Pause()
    {
        pauseMenu.SetActive(true);
    }
    public void Continue()
    {
        pauseMenu.SetActive(false);
        MainSystem.Instance.ChangeGameState(GameState.InGame);
    }
    public void OpenMainMenu()
    {
        loadingScreen.SetActive(true);
        if (SceneSystem.Instance == null)
        {
            Debug.LogWarning("SceneSystem.Instance == null");
        }
        SceneSystem.Instance.SwitchScene(SceneSystem.Scenes.MainMenu);
    }
    public void OnSceneLoading(float progress)
    {
        progressSlider.value = progress;
        progressText.text = progress * 100 + "%";
    }
}
